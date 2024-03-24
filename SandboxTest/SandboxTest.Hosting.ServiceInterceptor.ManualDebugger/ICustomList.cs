namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public interface ICustomList<T> : IList<T> where T: TestClass
    {
    }
}
