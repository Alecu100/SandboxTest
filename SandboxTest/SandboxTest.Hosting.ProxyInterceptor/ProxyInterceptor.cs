using System.Reflection;

namespace SandboxTest.ProxyWrapper
{
    public class ProxyInterceptor : DispatchProxy
    {
        private object? _wrappedInstance;
        private ProxyIncerpeptorController? _controller;


        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            throw new NotImplementedException();
        }

        public void Initialize(ProxyIncerpeptorController controller, object? wrappedInstance)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
        }
    }
}
