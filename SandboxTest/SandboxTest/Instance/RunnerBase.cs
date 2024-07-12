using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Base class for all runners that provides basic implementation of common functionality.
    /// </summary>
    public abstract class RunnerBase : IRunner
    {
        protected readonly List<AttachedDynamicMethod> _attachedDynamicMethods = new List<AttachedDynamicMethod>();
        protected bool _isRunning;

        /// <inheritdoc/>
        public IReadOnlyList<AttachedDynamicMethod> AttachedMethods => _attachedDynamicMethods;

        /// <inheritdoc/>
        public bool IsRunning { get => _isRunning; }

        /// <inheritdoc/>
        public virtual void AddAttachedDynamicMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order)
        {
            _attachedDynamicMethods.Add(new AttachedDynamicMethod(methodType, method, name, targetMethodName, order));
        }

        /// <inheritdoc/>
        public abstract Task ResetAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <inheritdoc/>
        public abstract Task RunAsync(IScenarioSuiteContext scenarioSuiteContext);

        /// <inheritdoc/>
        public abstract Task StopAsync(IScenarioSuiteContext scenarioSuiteContext);
    }
}
