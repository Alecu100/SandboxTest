using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Internal
{
    /// <summary>
    /// Special internal inteface, mostly not for public use to interact with the runtime and access information about the runtime context directly.
    /// </summary>
    public interface IRuntimeContext
    {
        /// <summary>
        /// Gets the controllers from the context.
        /// </summary>
        IReadOnlyCollection<IController> Controllers { get; }
        
        /// <summary>
        /// Gets the steps from the context.
        /// </summary>
        IReadOnlyCollection<ScenarioStep> Steps { get; }

        /// <summary>
        /// Gets the runner from the context.
        /// </summary>
        IRunner Runner { get; }

        /// <summary>
        /// Gets the instance from the context.
        /// </summary>
        IInstance Instance { get; }

        /// <summary>
        /// Executes an attached methods chain.
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="includedStepTypes"></param>
        /// <param name="targetMethod"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Task ExecuteAttachedMethodsChain(IEnumerable<object> instances, IEnumerable<AttachedMethodType> includedStepTypes, Delegate targetMethod, IEnumerable<object> arguments);
    }
}
