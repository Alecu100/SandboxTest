namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    public interface IListTestClass<T> : IList<T> where T : TestClass 
    {
    }
}
