namespace SandboxTest.Hosting.Tests
{
    public interface ICustomList<T> : IList<T> where T : TestClass
    {
    }
}
