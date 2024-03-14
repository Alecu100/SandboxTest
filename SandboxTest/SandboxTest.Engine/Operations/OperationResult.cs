namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// Represents the result of doing an operation.
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Gets the name of the type of the operation result
        /// </summary>
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// Creates a new instance of <see cref="OperationResult"/>
        /// </summary>
        /// <param name="isSuccesful">Denotes if the operation was succefully ran.</param>
        /// <param name="errorMessage">The error message in case the operation failed</param>
        public OperationResult(bool isSuccesful, string? errorMessage = default)
        {
            ErrorMessage = errorMessage;
            IsSuccesful = isSuccesful;
            TypeName = nameof(OperationResult);
        }

        /// <summary>
        /// Denotes if the operation was succesfully ran.
        /// </summary>
        public bool IsSuccesful { get; set; }

        /// <summary>
        /// Provides the error message in case the operation was not successful.
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
