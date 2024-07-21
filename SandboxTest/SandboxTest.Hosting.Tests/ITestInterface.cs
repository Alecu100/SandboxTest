namespace SandboxTest.Hosting.Tests
{
    public interface ITestInterface<T> where T : TestClass
    {
        T this[int i] { get; set; }

        int Count { get; }

        void Add(T instance);

        T? Get(int index);
    }
}
