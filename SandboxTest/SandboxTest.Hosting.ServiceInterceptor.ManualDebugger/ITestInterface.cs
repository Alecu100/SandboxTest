namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public interface ITestInterface<T> where T : TestClass
    {
        void Add(T instance);

        T Get(int index);
    }
}
