namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public interface IListTestClass<T> : IList<T> where T : TestClass 
    {
    }
}
