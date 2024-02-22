namespace SandboxTest.Engine.Operations
{
    /// <summary>
    /// Base class for sending commands between processes serialized in json using type information to deserialize it to the right type.
    /// </summary>
    public class Operation
    {
        /// <summary>
        /// Gets the name of the type of the operation
        /// </summary>
        public string TypeName { get; set; } = string.Empty;
    }
}
