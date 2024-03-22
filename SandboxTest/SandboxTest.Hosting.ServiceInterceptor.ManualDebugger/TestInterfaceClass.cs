﻿namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
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
    }
}
