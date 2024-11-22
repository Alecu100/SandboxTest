using System.Diagnostics;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Instance.Hosted;

namespace SandboxTest.Application
{
    /// <summary>
    /// Represents a hosted application instance that starts a new process locally dedicated to the instance.
    /// </summary>
    public class ApplicationHostedInstance : ApplicationInstance, IHostedInstance, IAttachedMethodContainer
    {
        protected readonly List<AttachedDynamicMethod> _attachedMethods = new List<AttachedDynamicMethod>();
        protected List<string>? _addresses;
        protected Process? _applicationInstanceProcess;
        protected IHostedInstanceMessageChannel? _messageChannel;
        protected bool _isPackaged = false;

        /// <summary>
        /// Creates an empty default application instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public new static ApplicationHostedInstance CreateEmptyInstance()
        {
            return new ApplicationHostedInstance();
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
        /// For application hosted instances, they run on the same machine so their address always resolves to 127.0.0.1.
        /// </summary>
        public IReadOnlyList<string> Addresses { get => _addresses ?? throw new InvalidOperationException("Appliction hosted instance not started"); }

        /// <summary>
        /// Gets and sets whether the instance should be packaged in a separate dedicated folder from the main test folder.
        /// </summary>
        public bool IsPackaged { get => _isPackaged; set => _isPackaged = value; }

        /// <inheritdoc/>
        public IReadOnlyList<AttachedDynamicMethod> AttachedMethods { get => _attachedMethods; }

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
            _addresses = new List<string> { "127.0.0.1" };
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

        /// <inheritdoc/>
        public void AddAttachedMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order)
        {
            if (_attachedMethods.Any(attachedMethod => attachedMethod.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException($"Attached method with {name} already added");
            }
            _attachedMethods.Add(new AttachedDynamicMethod(AttachedMethodType.HostedInstanceToHostedInstance, method, name, targetMethodName, order));
        }
    }
}
