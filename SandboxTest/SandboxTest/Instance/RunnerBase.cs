using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Base class for all runners that provides basic implementation of common functionality.
    /// </summary>
    public abstract class RunnerBase : IRunner
    {
        protected readonly List<AttachedDynamicMethod> _attachedDynamicMethods = new List<AttachedDynamicMethod>();

        public IReadOnlyList<AttachedDynamicMethod> AttachedMethods => _attachedDynamicMethods;

        /// <inheritdoc/>
        public virtual void AddAttachedDynamicMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order)
        {
            _attachedDynamicMethods.Add(new AttachedDynamicMethod(methodType, method, name, targetMethodName, order));
        }

        /// <inheritdoc/>
        public abstract Task ResetAsync();

        /// <inheritdoc/>
        public abstract Task RunAsync();

        /// <inheritdoc/>
        public abstract Task StopAsync();
    }
}
