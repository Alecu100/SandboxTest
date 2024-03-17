using System.Reflection;

namespace SandboxTest.ProxyWrapper
{
    public class ProxyInterceptorRecordedCall
    {
        private Type _interfaceType;
        private MethodInfo _method;
        private object?[]? _args;

        public ProxyInterceptorRecordedCall(Type interfaceType, MethodInfo method, object?[]? args = null)
        {
            _interfaceType = interfaceType;
            _method = method;
            _args = args;
        }
    }
}
