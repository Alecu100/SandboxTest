using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Internal;
using SandboxTest.Scenario;

namespace SandboxTest.Engine.ChildTestEngine
{
    /// <summary>
    /// Represents a way to access the internal runtime context, used for internal use only.
    /// </summary>
    public class RuntimeContext : IRuntimeContext
    {
        private readonly IChildTestEngine _childTestEngine;

        /// <summary>
        /// Creates a new instance of <see cref="RuntimeContext"/>.
        /// </summary>
        /// <param name="instance"></param>
        public RuntimeContext(IChildTestEngine instance)
        {
            _childTestEngine = instance;
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IController> Controllers { get => _childTestEngine?.RunningInstance?.Controllers ?? throw new InvalidOperationException("Instance has no runner assigned"); }

        /// <inheritdoc/>
        public IReadOnlyCollection<ScenarioStep> Steps { get => _childTestEngine?.RunningInstance?.Steps ?? throw new InvalidOperationException("Instance has no runner assigned"); }

        /// <inheritdoc/>
        public IRunner Runner { get => _childTestEngine?.RunningInstance?.Runner ?? throw new InvalidOperationException("Instance has no runner assigned") ?? throw new InvalidOperationException("Instance has no runner assigned"); }

        /// <inheritdoc/>
        public IInstance Instance { get => _childTestEngine?.RunningInstance ?? throw new InvalidOperationException("Instance has no runner assigned"); }

        /// <inheritdoc/>
        public IScenarioSuiteContext ScenarioSuiteContext { get => _childTestEngine?.ScenarioSuiteContext ?? throw new InvalidOperationException("No scenario suite context found"); }

        /// <inheritdoc/>
        public async Task ExecuteAttachedMethodsChain(IEnumerable<object> instances, IEnumerable<AttachedMethodType> includedStepTypes, Delegate targetMethod, IEnumerable<object> arguments)
        {
            await _childTestEngine.AttachedMethodsExecutor.ExecuteAttachedMethodsChain(instances, includedStepTypes, targetMethod, arguments);
        }
    }
}
