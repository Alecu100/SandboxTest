namespace SandboxTest.Engine.Operations
{
    public class ReadyOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="ResetApplicationInstanceOperation"/>
        /// </summary>
        public ReadyOperation(string applicationInstanceId)
        {
            TypeName = nameof(ReadyOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance to wait for for to be ready and loaded.
        /// </summary>
        public string InstanceId { get; private set; }
    }
}
