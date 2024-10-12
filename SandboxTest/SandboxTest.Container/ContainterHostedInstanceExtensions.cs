using SandboxTest.Instance.Hosted;

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
        public static IHostedInstance ConfigureContainerHostedInstance(this IHostedInstance hostedInstance, Func<ContainerHostedInstance, IHostedInstanceContext, Task>? configureBuildFunc)
        {
            var containterHostedInstance = hostedInstance as ContainerHostedInstance;
            if (containterHostedInstance == null)
            {
                throw new InvalidOperationException("Invalid hosted instance type, expected container hosted instance");
            }

            containterHostedInstance.OnConfigureBuild(configureBuildFunc);
            return containterHostedInstance;
        }
    }
}
