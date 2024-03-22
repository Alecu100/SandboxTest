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
            Console.WriteLine("Hello, World!");
        }
    }
}
