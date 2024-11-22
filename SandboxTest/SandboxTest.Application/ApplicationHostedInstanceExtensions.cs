using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;
using SandboxTest.Utils;

namespace SandboxTest.Application
{
    public static class ApplicationHostedInstanceExtensions
    {
        /// <summary>
        /// Assigns the default <see cref="ApplicationHostedInstanceMessageChannel"/> to use as a message channel to a <see cref="ApplicationHostedInstance"/>. 
        /// </summary>
        /// <returns></returns>
        public static ApplicationHostedInstance UseApplicationHostedInstanceMessageChannel(this ApplicationHostedInstance applicationHostedInstance)
        {
            applicationHostedInstance.UseMessageChannel(new ApplicationHostedInstanceMessageChannel());
            return applicationHostedInstance;
        }

        /// <summary>
        /// Packages additional files from another directory such as appsettings.json files or razor pages files.
        /// </summary>
        /// <param name="containerHostedInstance">The container hosted instance to generate a separate package directory copying the specified directory in the package directory.</param>
        /// <param name="fullDirectoryName">The full name and path of the directory from which to copy the files and sub directories.</param>
        /// <param name="filesToIgnore">Files and sub directories to ignore when copying, wildcards at the beginning or end can be used to specify files or directory that start, end or contain a specific text.</param>
        /// <returns></returns>
        public static ApplicationHostedInstance PackageFilesFromDirectory(this ApplicationHostedInstance applicationHostedInstance, string fullDirectoryName, params string[]? filesToIgnore)
        {
            applicationHostedInstance.IsPackaged = true;
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
            Func<IHostedInstanceContext, Task> onPackageFilesFromDirectory = async (ctx) =>
            {
                await PathUtils.CopyDirectoryAsync(fullDirectoryName, ctx.PackageFolder!, true, default, filters?.ToArray());
            };
            var attachedMethodName = $"{nameof(onPackageFilesFromDirectory)}_{fullDirectoryName.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.VolumeSeparatorChar, '_')}";
            applicationHostedInstance.AddAttachedMethod(AttachedMethodType.HostedInstanceToHostedInstance, onPackageFilesFromDirectory, attachedMethodName, nameof(applicationHostedInstance.StartAsync), -100);
            return applicationHostedInstance;
        }
    }
}
