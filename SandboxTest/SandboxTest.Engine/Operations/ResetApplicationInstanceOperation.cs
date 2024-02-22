namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// An operation that represents reseting an application instance after a scenario is ran.
    /// </summary>
    public class ResetApplicationInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="ResetApplicationInstanceOperation"/>
        /// </summary>
        public ResetApplicationInstanceOperation(string applicationInstanceId) 
        {
            TypeName = nameof(ResetApplicationInstanceOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance id that should be reset, mostly used to verify that the reset was sent to the right application instance host.
        /// </summary>
        public string InstanceId { get; set; }
    }
}
