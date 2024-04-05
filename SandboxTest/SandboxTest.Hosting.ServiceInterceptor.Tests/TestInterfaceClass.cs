namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    public class TestInterfaceClass<T> : ITestInterface<T> where T : TestClass
    {
        private readonly List<T> _list = new List<T>();

        public TestInterfaceClass()
        {
        }

        public TestInterfaceClass(List<T> list)
        {
            _list = list;
        }

        public void Add(T instance)
        {
            _list.Add(instance);
        }

        public T? Get(int index)
        {
            if (_list.Count <= index)
            {
                return default;
            }
            return _list[index];
        }

        public int Count
        {
            get
            {
                return _list.Count;
            }
        }

        public T this[int i]
        {
            get { return _list[i]; }
            set { _list[i] = value; }
        }

    }
}
