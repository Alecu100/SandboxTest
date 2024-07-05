namespace SandboxTest.Instance.AttachedMethod
{
    public interface IAttachedMethodContainer
    {
        /// <summary>
        /// Gets the list of attached dynamic methods.
        /// </summary>
        IReadOnlyList<AttachedDynamicMethod> AttachedMethods { get; }

        /// <summary>
        /// Adds a attached dynamic method.
        /// </summary>
        /// <param name="methodType"></param>
        /// <param name="method"></param>
        /// <param name="name"></param>
        /// <param name="targetMethodName"></param>
        /// <param name="order"></param>
        void AddAttachedDynamicMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order);
    }
}
