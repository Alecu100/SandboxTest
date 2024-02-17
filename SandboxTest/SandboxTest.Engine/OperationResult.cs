namespace SandboxTest.Engine
{
    /// <summary>
    /// Represents the result of doing an operation.
    /// </summary>
    public class OperationResult
    {
        private readonly bool _isSuccesful;
        private readonly string? _error;

        public OperationResult(bool isSuccesful, string? error) 
        { 
            _error = error;
            _isSuccesful = isSuccesful;
        }

        public bool IsSuccesful { get => _isSuccesful; } 
        
        public string? Error { get => _error; }
    }
}
