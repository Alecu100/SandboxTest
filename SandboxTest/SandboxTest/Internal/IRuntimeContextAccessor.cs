namespace SandboxTest.Internal
{
    /// <summary>
    /// Interface for internal use allowing implementing classes to have the runtime context injected.
    /// </summary>
    public interface IRuntimeContextAccessor
    {
        void InitializeContext(IRuntimeContext context);
    }
}
