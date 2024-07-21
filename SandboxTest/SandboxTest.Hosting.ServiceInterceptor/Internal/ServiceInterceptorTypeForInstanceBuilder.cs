
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.ServiceInterceptor.Internal
{
    public class ServiceInterceptorTypeForInstanceBuilder : ServiceInterceptorTypeBuilderBase
    {
        public ServiceInterceptorTypeForInstanceBuilder(Type interfaceType, GCHandle serviceInterceptorHandle, ServiceInterceptorAssembly serviceInterceptorAssembly) : base(interfaceType, serviceInterceptorHandle, serviceInterceptorAssembly)
        {
        }

        public override Type Build()
        {
            var guid = Guid.NewGuid();
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            _serviceInterceptorTypeBuilder = _serviceInterceptorAssembly.ModuleBuilder.DefineType($"ServiceInterceptor-{MakeSafeName(_interfaceType.Name)}-{guid}", TypeAttributes.Public | TypeAttributes.Class, serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap = null;
            _builtInterfaceMethods = new Dictionary<MethodInfo, MethodBuilder>();

            if (_interfaceType.IsGenericTypeDefinition)
            {
                var interfaceGenericArguments = _interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = _serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                serviceInterceptorGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
                }

                _implementedInterfaceType = ReplaceGenericArgumentsAndConstraintsFromType(_interfaceType, serviceInterceptorGenericParametersMap);
            }
            else
            {
                _implementedInterfaceType = _interfaceType;
            }

            _serviceInterceptorTypeBuilder.AddInterfaceImplementation(_implementedInterfaceType);

            _allImplementedInterfaces = GetAllImplementedInterfaces(_interfaceType);

            GenerateConstructor();

            GenerateInterfaceMethods();

            GenerateInterfaceProperties();

            GenerateInterfaceEvents();

            return _serviceInterceptorTypeBuilder.CreateType()!;
        }

        private void GenerateConstructor()
        {
            var ptrHandleServiceInterceptorController = GCHandle.ToIntPtr(_serviceInterceptorHandle);
            var baseConstructor = _serviceInterceptorBaseType.GetConstructor(new[] { typeof(nint), typeof(object) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var constructor = _serviceInterceptorTypeBuilder!.DefineConstructor(baseConstructor.Attributes, baseConstructor.CallingConvention, new[] { typeof(object) });
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.EmitWriteLine("Calling generated constructor for interface type");
            ilGenerator.Emit(OpCodes.Ldarg_0);
            if (IntPtr.Size == 4)
                ilGenerator.Emit(OpCodes.Ldc_I4, ptrHandleServiceInterceptorController.ToInt32());
            else
                ilGenerator.Emit(OpCodes.Ldc_I8, ptrHandleServiceInterceptorController.ToInt64());
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Call, baseConstructor);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ret);
        }
    }
}
