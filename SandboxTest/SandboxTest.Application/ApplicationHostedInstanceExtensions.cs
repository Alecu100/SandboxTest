namespace SandboxTest.Application
{
    public static class ApplicationHostedInstanceExtensions
    {
        /// <summary>
        /// Assigns the default <see cref="ApplicationHostedInstanceMessageChannel"/> to use as a message channel to a <see cref="ApplicationHostedInstance"/>. 
        /// </summary>
        /// <returns></returns>
        public static ApplicationHostedInstance UseDefaultApplicationHostedInstanceMessageChannel(this ApplicationHostedInstance applicationHostedInstance)
        {
            applicationHostedInstance.MessageChannel = new ApplicationHostedInstanceMessageChannel();
            return applicationHostedInstance;
        }
    }
}
