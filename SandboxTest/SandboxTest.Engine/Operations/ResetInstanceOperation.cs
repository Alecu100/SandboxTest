namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// An operation that represents reseting an instance after a scenario is ran.
    /// </summary>
    public class ResetInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="ResetInstanceOperation"/>
        /// </summary>
        public ResetInstanceOperation(string applicationInstanceId) 
        {
            TypeName = nameof(ResetInstanceOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance id that should be reset, mostly used to verify that the reset was sent to the right application instance host.
        /// </summary>
        public string InstanceId { get; set; }
    }
}
