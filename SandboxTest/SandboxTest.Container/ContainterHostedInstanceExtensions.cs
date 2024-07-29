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
    }
}
