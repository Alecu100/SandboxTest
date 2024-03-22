namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public class TestInterfaceClass<T> : ITestInterface<T> where T : TestClass
    {
        private readonly List<T> _list = new List<T>();

        public void Add(T instance)
        {
            _list.Add(instance);
        }

        public T Get(int index)
        {
            return _list[index];
        }
    }
}
