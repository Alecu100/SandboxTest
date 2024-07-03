using System.Diagnostics;

namespace SandboxTest
{
    /// <summary>
    /// Represents a hosted application instance that starts a new process dedicated to the instance.
    /// </summary>
    public class ApplicationHostedInstance : ApplicationInstance, IHostedInstance
    {
        private Process? _applicationInstanceProcess;

        private IHostedInstanceMessageChannel? _messageChannel;

        /// <summary>
        /// Creates an empty default application instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ApplicationHostedInstance CreateEmptyHostedInstance(string id)
        {
            return new ApplicationHostedInstance(id);
        }

        public ApplicationHostedInstance(string id) : base(id) 
        {

        }

        /// <inheritdoc/>
        public void UseMessageChannel(IHostedInstanceMessageChannel messageChannel)
        {
            if (_messageChannel != null)
            {
                throw new InvalidOperationException("Application hosted instance already has a message channel assigned");
            }
            _messageChannel = messageChannel;
        }

        /// <inheritdoc/>
        public virtual IHostedInstanceMessageChannel? MessageChannel { get => _messageChannel; }

        /// <summary>
        /// Starts the host for the application instance from the command line.
        /// </summary>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual async Task StartAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData, CancellationToken token)
        {
            var applicationRunnerPath = $"{instanceData.MainPath}\\SandboxTest.Application.exe";

            _applicationInstanceProcess = await instanceContext.LaunchProcessAsync(applicationRunnerPath, instanceContext.IsBeingDebugged, instanceData.MainPath, string.Join(' ', instanceData.ToCommandLineArguments()));
        }

        /// <summary>
        /// Stops the host for the application instance.
        /// </summary>
        /// <param name="instanceContext"></param>
        /// <param name="instanceData"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Task StopAsync(IHostedInstanceContext instanceContext, HostedInstanceData instanceData)
        {
            if (_applicationInstanceProcess == null)
            {
                throw new InvalidOperationException("Application instance process not started");
            }
            _applicationInstanceProcess.Kill(true);
            return Task.CompletedTask;
        }
    }
}
