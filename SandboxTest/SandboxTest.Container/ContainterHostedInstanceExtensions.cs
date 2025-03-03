using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using SandboxTest.Utils;

namespace SandboxTest.Container
{
    public static class ContainerHostedInstanceExtensions
    {
        /// <summary>
        /// Assigns the default <see cref="ContainerHostedInstanceMessageChannel"/> to use as a message channel to a <see cref="ContainerHostedInstance"/>. 
        /// </summary>
        /// <returns></returns>
        public static ContainerHostedInstance UseContainerHostedInstanceMessageChannel(this ContainerHostedInstance containerHostedInstance, short port = 6789)
        {
            containerHostedInstance.UseMessageChannel(new ContainerHostedInstanceMessageChannel(port));
            return containerHostedInstance;
        }

        /// <summary>
        /// Configures the = to use when creating the <see cref="ContainerHostedInstance"/> for a <see cref="ContainerHostedInstance"/>.
        /// </summary>
        /// <param name="configureBuildFunc">The function to call to configure the hosted instance</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static ContainerHostedInstance ConfigureContainerHostedInstance(this IHostedInstance hostedInstance, Func<ContainerHostedInstance, IHostedInstanceContext, Task>? configureBuildFunc)
        {
            var containterHostedInstance = hostedInstance as ContainerHostedInstance;
            if (containterHostedInstance == null)
            {
                throw new InvalidOperationException("Invalid hosted instance type, expected container hosted instance");
            }

            containterHostedInstance.OnConfigureBuild(configureBuildFunc);
            return containterHostedInstance;
        }

        /// <summary>
        /// Packages additional files from another directory such as appsettings.json files or razor pages files.
        /// </summary>
        /// <param name="containerHostedInstance">The container hosted instance to generate a separate package directory copying the specified directory in the package directory.</param>
        /// <param name="fullDirectoryName">The full name and path of the directory from which to copy the files and sub directories.</param>
        /// <param name="filesToIgnore">Files and sub directories to ignore when copying, wildcards at the beginning or end can be used to specify files or directory that start, end or contain a specific text.</param>
        /// <returns></returns>
        public static ContainerHostedInstance PackageFilesFromDirectory(this ContainerHostedInstance containerHostedInstance, string fullDirectoryName, params string[]? filesToIgnore)
        {
            containerHostedInstance.IsPackaged = true;
            List<Func<string, bool>>? filters = null;
            if (filesToIgnore != null && filesToIgnore.Any())
            {
                filters = new List<Func<string, bool>>();
                foreach (var fileToIgnore in filesToIgnore)
                {
                    if (fileToIgnore.StartsWith("*", StringComparison.InvariantCultureIgnoreCase) && fileToIgnore.EndsWith("*", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filters.Add(name => !name.Contains(fileToIgnore.Trim('*')));
                        continue;
                    }
                    if (fileToIgnore.StartsWith("*", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filters.Add(name => !name.EndsWith(fileToIgnore.Trim('*')));
                        continue;
                    }
                    if (fileToIgnore.EndsWith("*", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filters.Add(name => !name.StartsWith(fileToIgnore.Trim('*')));
                        continue;
                    }
                    filters.Add(name => !name.Equals(fileToIgnore, StringComparison.InvariantCultureIgnoreCase));
                    continue;
                }
            }
            var onPackageFilesFromDirectory = async (IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token) =>
            {
                await PathUtils.CopyDirectoryAsync(fullDirectoryName, instanceContext.PackageFolder!, true, default, filters?.ToArray());
            };
            var attachedMethodName = $"{nameof(onPackageFilesFromDirectory)}_{fullDirectoryName.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_')}";
            containerHostedInstance.AddAttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, onPackageFilesFromDirectory, attachedMethodName, nameof(containerHostedInstance.BuildAsync), -100);
            return containerHostedInstance;
        }

        /// <summary>
        /// Packages additional files from another directory such as appsettings.json files or razor pages files.
        /// </summary>
        /// <param name="containerHostedInstance">The container hosted instance to generate a separate package directory copying the specified directory in the package directory.</param>
        /// <param name="fullDirectoryName">The full name and path of the directory from which to copy the files and sub directories.</param>
        /// <param name="filter">Filter for files and directories to include.</param>
        /// <returns></returns>
        public static ContainerHostedInstance PackageFilesFromDirectory(this ContainerHostedInstance containerHostedInstance, string fullDirectoryName, Func<string, bool> filter)
        {
            containerHostedInstance.IsPackaged = true;
            Func<IHostedInstanceContext, Task> onPackageFilesFromDirectory = async (ctx) =>
            {
                await PathUtils.CopyDirectoryAsync(fullDirectoryName, ctx.PackageFolder!, true, default, new[] { filter });
            };
            var attachedMethodName = $"{nameof(onPackageFilesFromDirectory)}_{fullDirectoryName.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_')}";
            containerHostedInstance.AddAttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, onPackageFilesFromDirectory, attachedMethodName, nameof(containerHostedInstance.StartAsync), -100);
            return containerHostedInstance;
        }
    }
}
