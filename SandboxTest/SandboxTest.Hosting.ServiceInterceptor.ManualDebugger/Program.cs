using SandboxTest.Hosting.ServiceInterceptor;

namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceInterceptorController = new ServiceInterceptorController();
            var serviceInterceptorListType = ServiceInterceptor.CreateServiceInterceptorClassWrapper(typeof(ITestInterface<>), typeof(TestInterfaceClass<>), serviceInterceptorController);
            var concreteType = serviceInterceptorListType.MakeGenericType(new Type[] { typeof(TestClassDerived) });
            var instance = Activator.CreateInstance(concreteType, new object[] { serviceInterceptorController }) as ITestInterface<TestClassDerived>;
            instance.Get(0);
            //instance.Add(new TestClassDerived());
            instance = Activator.CreateInstance(concreteType, new object[] { serviceInterceptorController, new List<TestClassDerived>() { new TestClassDerived { Name = "Asda" } } }) as ITestInterface<TestClassDerived>;
            Console.WriteLine("Hello, World!");
        }
    }
}
