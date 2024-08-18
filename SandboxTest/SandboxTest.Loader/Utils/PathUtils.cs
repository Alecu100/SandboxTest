namespace SandboxTest.Loader.Utils
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
