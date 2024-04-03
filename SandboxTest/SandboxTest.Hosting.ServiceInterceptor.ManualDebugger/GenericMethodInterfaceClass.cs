namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    public class GenericMethodInterfaceClass : IGenericMethodInterface
    {
        public Task PrintToConsoleGeneric<T>(T obj) where T : TestClass
        {
            Console.WriteLine(obj.Name);
            return Task.CompletedTask;
        }
    }
}
