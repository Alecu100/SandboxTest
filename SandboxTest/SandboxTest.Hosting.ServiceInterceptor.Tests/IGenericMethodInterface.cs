namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public interface IGenericMethodInterface
    {
        Task PrintToConsoleGeneric<T>(T obj) where T : TestClass;
    }
}
