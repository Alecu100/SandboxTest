using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Base class for all runners that provides basic implementation of common functionality.
    /// </summary>
    public abstract class RunnerBase : IRunner
    {
        protected readonly List<AttachedDynamicMethod> _attachedMethods = new List<AttachedDynamicMethod>();
        protected bool _isRunning;

        /// <inheritdoc/>
        public IReadOnlyList<AttachedDynamicMethod> AttachedMethods => _attachedMethods;

        /// <inheritdoc/>
        public bool IsRunning { get => _isRunning; }

        /// <inheritdoc/>
        public virtual void AddAttachedMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order)
        {
            if (_attachedMethods.Any(attachedMethod => attachedMethod.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new InvalidOperationException($"Runner already has an attached method with the name {name}");
            }
            _attachedMethods.Add(new AttachedDynamicMethod(methodType, method, name, targetMethodName, order));
        }

        /// <inheritdoc/>
        public abstract Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <inheritdoc/>
        public abstract Task RunAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <inheritdoc/>
        public abstract Task StopAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
