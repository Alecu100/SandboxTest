namespace SandboxTest
{
    /// <summary>
    /// Applied to controller, runner or host instance methods to execute them before or after a target method.
    /// </summary>
    public class AttachedMethodAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AttachedMethodAttribute"/>
        /// </summary>
        /// <param name="injectedStepType"></param>
        /// <param name="order"></param>
        public AttachedMethodAttribute(AttachedMethodType injectedStepType, string targetMethodName, int order) 
        {
            InjectedStepType = injectedStepType;
            TargetMethodName = targetMethodName;
            Order = order;
        }

        /// <summary>
        /// Gets the step execution type, before or after the main runner methods stop or run.
        /// </summary>
        public AttachedMethodType InjectedStepType { get; set; }

        /// <summary>
        /// Represents the name of the target method to execute the step along.
        /// </summary>
        public string TargetMethodName { get; set; }

        /// <summary>
        /// Represents the order in which the injected step will be executed, a negative value means that it will be executed before the target method and a positive value means it will be executed after.
        /// If there are multiple injected steps, they will be executed starting with the lowest order values and ending with the highest order values.
        /// </summary>
        public int Order { get; set; }
    }
}
