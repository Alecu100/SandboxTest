namespace SandboxTest.Utils
{
    public static class PathUtils
    {
        /// <summary>
        /// Locates a folder inside the current work directory path and returns the path to that folder.
        /// </summary>
        /// <param name="folderPathToFind"></param>
        /// <returns></returns>
        public static string? LocateFolderPath(string folderPathToFind)
        {
            return LocateFolderPath(Environment.CurrentDirectory, folderPathToFind);
        }

        /// <summary>
        /// Locates a folder inside a path and returns the path to that folder.
        /// </summary>
        /// <param name="folderPathToFind">The path to search for the folder in and return the path to the found folder.</param>
        /// <param name="fullPathToSearchIn">The folder for which to search the path.</param>
        /// <returns></returns>
        public static string? LocateFolderPath(string fullPathToSearchIn, string folderPathToFind)
        {
            if (fullPathToSearchIn.EndsWith(folderPathToFind, StringComparison.InvariantCultureIgnoreCase))
            {
                return fullPathToSearchIn;
            }

            if (fullPathToSearchIn.EndsWith(Path.DirectorySeparatorChar))
            {
                fullPathToSearchIn = fullPathToSearchIn.TrimEnd(Path.DirectorySeparatorChar);
            }

            var pathSeparatorPosition = fullPathToSearchIn.LastIndexOf(Path.DirectorySeparatorChar);
            if (pathSeparatorPosition < 0)
            {
                return null;
            }

            return LocateFolderPath(fullPathToSearchIn.Substring(0, pathSeparatorPosition), folderPathToFind);
        }

        /// <summary>
        /// Copies all the contents from a source directory to a destination directory.
        /// </summary>
        /// <param name="sourceDirectory">The absolute path of the source directory.</param>
        /// <param name="destinationDirectory">The absolute path of the destination directory.</param>
        /// <param name="includeSubDirectories">Copies all sub directories if set to true.</param>
        /// <param name="cancellationToken">A cancellation token to stop copying of the directory.</param>
        /// <param name="filters">Optional filter expression to only copy certain files and directories.</param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static async Task CopyDirectoryAsync(string sourceDirectory, string destinationDirectory, bool includeSubDirectories, CancellationToken cancellationToken = default, params Func<string, bool>[]? filters)
        {
            var currentDirectory = new DirectoryInfo(sourceDirectory);

            if (!currentDirectory.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirectory);
            }

            var directoriesToCopy = currentDirectory.GetDirectories();
            if (!Directory.Exists(destinationDirectory))
            {
                Directory.CreateDirectory(destinationDirectory);
            }

            var filesToCopy = currentDirectory.GetFiles();
            var allCopyTasks = new List<Task>();
            if (filters != null && filters.Any())
            {
                filesToCopy = filesToCopy.Where(file => filters.Any(filter => filter(file.FullName))).ToArray();
            }
            allCopyTasks.Add(Parallel.ForEachAsync(filesToCopy, cancellationToken, async (fileInfo, token) =>
            {
                var fileCopyPath = Path.Combine(destinationDirectory, fileInfo.Name);
                await CopyFileAsync(fileInfo.FullName, fileCopyPath, token);
            }));

            if (includeSubDirectories)
            {
                if (filters != null && filters.Any())
                {
                    directoriesToCopy = directoriesToCopy.Where(directory => filters.Any(filter => filter(directory.FullName))).ToArray();
                }
                allCopyTasks.Add(Parallel.ForEachAsync(directoriesToCopy, cancellationToken, async (directoryInfo, token) =>
                {
                    var directoryCopyPath = Path.Combine(destinationDirectory, directoryInfo.Name);
                    await CopyDirectoryAsync(directoryInfo.FullName, directoryCopyPath, includeSubDirectories, cancellationToken, filters);
                }));
            }

            await Task.WhenAll(allCopyTasks);
        }

        public static async Task CopyFileAsync(string sourceFile, string destinationFile, CancellationToken cancellationToken = default)
        {
            var fileOptions = FileOptions.Asynchronous | FileOptions.SequentialScan;
            var bufferSize = 4096;

            using (var sourceStream =
                  new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, fileOptions))

            using (var destinationStream =
                  new FileStream(destinationFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, fileOptions))

                await sourceStream.CopyToAsync(destinationStream, bufferSize, cancellationToken)
                                  .ConfigureAwait(false);
        }

        /// <summary>
        /// Returns only the name of a directory without the path before it.
        /// </summary>
        /// <param name="path">The full directory name, including the path to it.</param>
        /// <returns></returns>
        public static string GetDirectoryNameOnly(string path)
        {
            var searchedSpan = path.AsSpan();
            var lastIndexOfSeparator = searchedSpan.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastIndexOfSeparator < 0)
            {
                return new string(searchedSpan);
            }
            if (lastIndexOfSeparator == searchedSpan.Length - 1)
            {
                searchedSpan = searchedSpan.Slice(0, searchedSpan.Length - 1);
            }
            lastIndexOfSeparator = searchedSpan.LastIndexOf(Path.DirectorySeparatorChar);
            if (lastIndexOfSeparator < 0)
            {
                return new string(searchedSpan);
            }
            return new string(searchedSpan.Slice(lastIndexOfSeparator + 1));
        }

        /// <summary>
        /// Appends to a path additional parts such as folders or a file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="partsToAppend"></param>
        /// <returns></returns>
        public static string AppendToPath(string path, params string[] partsToAppend)
        {
            var pathParts = new List<string>();
            pathParts.Add(path);
            pathParts.AddRange(partsToAppend);
            return string.Join(Path.DirectorySeparatorChar, pathParts);
        }
    }
}
