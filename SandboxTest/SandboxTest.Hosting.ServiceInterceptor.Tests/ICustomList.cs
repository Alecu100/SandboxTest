namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    public interface ICustomList<T> : IList<T> where T : TestClass
    {
    }
}
