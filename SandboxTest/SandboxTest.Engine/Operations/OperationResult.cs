namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// Represents the result of doing an operation.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Creates a new instance of <see cref="OperationResult"/>
        /// </summary>
        /// <param name="isSuccesful">Denotes if the operation was succefully ran.</param>
        /// <param name="errorMessage">The error message in case the operation failed</param>
        public OperationResult(bool isSuccesful, string? errorMessage)
        {
            ErrorMessage = errorMessage;
            IsSuccesful = isSuccesful;
        }

        public bool IsSuccesful { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
