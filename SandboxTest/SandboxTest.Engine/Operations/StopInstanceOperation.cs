namespace SandboxTest.Engine.Operations
{
    public class StopInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="StopInstanceOperation"/>
        /// </summary>
        public StopInstanceOperation(string applicationInstanceId)
        {
            TypeName = nameof(StopInstanceOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance id that should be stopped, mostly used to verify that the stop request was sent to the right application instance host.
        /// </summary>
        public string InstanceId { get; set; }
    }
}
