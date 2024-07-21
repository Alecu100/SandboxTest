namespace SandboxTest.Hosting.Tests
{
    public interface IGenericMethodInterface
    {
        Task PrintToConsoleGeneric<T>(T obj) where T : TestClass;
    }
}
