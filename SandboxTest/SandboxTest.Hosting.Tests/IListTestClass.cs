namespace SandboxTest.Hosting.Tests
{
    public interface IListTestClass<T> : IList<T> where T : TestClass
    {
    }
}
