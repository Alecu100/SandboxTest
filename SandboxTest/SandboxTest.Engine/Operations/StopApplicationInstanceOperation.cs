namespace SandboxTest.Engine.Operations
{
    public class StopApplicationInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="StopApplicationInstanceOperation"/>
        /// </summary>
        public StopApplicationInstanceOperation(string applicationInstanceId)
        {
            TypeName = nameof(StopApplicationInstanceOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance id that should be stopped, mostly used to verify that the stop request was sent to the right application instance host.
        /// </summary>
        public string InstanceId { get; set; }
    }
}
