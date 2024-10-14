namespace SandboxTest.Instance.AttachedMethod
{
    /// <summary>
    /// Represents an dynamically attached method at runtime to be executed.
    /// </summary>
    public class AttachedDynamicMethod
    {
        private readonly Delegate _method;

        private readonly AttachedMethodType _methodType;

        private readonly string _targetMethodName;

        private readonly int _order;

        private readonly string _name;

        /// <summary>
        /// Creates a new instance of <see cref="AttachedDynamicMethod"/>
        /// </summary>
        /// <param name="methodType"></param>
        /// <param name="method"></param>
        /// <param name="name"></param>
        /// <param name="targetMethodName"></param>
        /// <param name="order"></param>
        public AttachedDynamicMethod(AttachedMethodType methodType, Delegate method, string name, string targetMethodName, int order)
        {
            _methodType = methodType;
            _method = method;
            _name = name;
            _targetMethodName = targetMethodName;
            _order = order;
        }

        /// <summary>
        /// Represents the actual attached method that will get executed.
        /// </summary>
        public Delegate MethodDelegate { get => _method; }

        /// <summary>
        /// Represents the type of the attached method.
        /// </summary>
        public AttachedMethodType MethodType { get => _methodType; }

        /// <summary>
        /// Represents the target method to attach the dynamic method to.
        /// </summary>
        public string TargetMethodName => _targetMethodName;

        /// <summary>
        /// Represents the order in which to execute the dynamically attached method, before the target method when the order is negative or after the target method when the order is greater or equal to zero.
        /// </summary>
        public int Order => _order;

        /// <summary>
        /// Represents the name of the attached method.
        /// </summary>
        public string Name => _name;
    }
}
