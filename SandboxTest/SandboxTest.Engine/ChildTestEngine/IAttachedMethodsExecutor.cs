using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Engine.ChildTestEngine
{
    /// <summary>
    /// Executes a target method along with a list of attached methods using the <see cref="AttachedMethodAttribute"/> attribute.
    /// </summary>
    public interface IAttachedMethodsExecutor
    {
        /// <summary>
        /// Runs a target method along with all of it's attached methods
        /// </summary>
        /// <param name="instances"></param>
        /// <param name="includedStepTypes"></param>
        /// <param name="targetMethod"></param>
        /// <returns></returns>
        Task ExecuteAttachedMethodsChain(IEnumerable<object> instances, IEnumerable<AttachedMethodType> includedStepTypes, Delegate targetMethod, IEnumerable<object> arguments);
    }
}
