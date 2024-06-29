namespace SandboxTest.Engine.Operations
{
    public class RunInstanceOperation : Operation
    {
        /// <summary>
        /// Create a new instance of <see cref="ResetInstanceOperation"/>
        /// </summary>
        public RunInstanceOperation(string applicationInstanceId)
        {
            TypeName = nameof(RunInstanceOperation);
            InstanceId = applicationInstanceId;
        }

        /// <summary>
        /// The application instance to wait for for to be ready and loaded.
        /// </summary>
        public string InstanceId { get; private set; }
    }
}
