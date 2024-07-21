using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.Internal
{
    public class ServiceInterceptorTypeForTypeBuilder : ServiceInterceptorTypeBuilderBase
    {
        private readonly Type _wrappedType;

        public ServiceInterceptorTypeForTypeBuilder(Type interfaceType, Type wrappedType, GCHandle serviceInterceptorHandle, ServiceInterceptorAssembly serviceInterceptorAssembly) : base(interfaceType, serviceInterceptorHandle, serviceInterceptorAssembly)
        {
            _wrappedType = wrappedType;
        }

        public override Type Build()
        {
            if (!GetAllInterfacesImplementedByType(_wrappedType!).Any(wrappedInterfaceType => InterfacesAreEquivalent(wrappedInterfaceType, _interfaceType)))
            {
                throw new InvalidOperationException($"Wrapped type {_wrappedType!.FullName} must implement interface type {_interfaceType.FullName}");
            }
            var guid = Guid.NewGuid();
            var serviceInterceptorTypeName = $"ServiceInterceptor-{MakeSafeName(_interfaceType.Name)}-{MakeSafeName(_wrappedType!.Name)}-{guid}";
            if (_interfaceType.IsGenericTypeDefinition)
            {
                serviceInterceptorTypeName = $"ServiceInterceptor-Generic-{MakeSafeName(_interfaceType.Name)}-{MakeSafeName(_wrappedType.Name)}-{guid}";
            }
            _serviceInterceptorTypeBuilder = _serviceInterceptorAssembly.ModuleBuilder.DefineType(serviceInterceptorTypeName, TypeAttributes.Public | TypeAttributes.Class, _serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap = null;
            _builtInterfaceMethods = new Dictionary<MethodInfo, MethodBuilder>();

            if (_interfaceType.IsGenericTypeDefinition)
            {
                if (!_wrappedType.IsGenericTypeDefinition)
                {
                    throw new InvalidOperationException("Wrapped type is not an open generic type whereas the interface is");
                }
                var interfaceGenericArguments = _interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = _serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                _serviceInterceptorGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                wrappedTypeGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    _serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
                }
                var wrappedTypeGenericArguments = _wrappedType.GetGenericArguments();
                for (int i = 0; i < wrappedTypeGenericArguments.Length; i++)
                {
                    wrappedTypeGenericParametersMap[wrappedTypeGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
                }

                _implementedInterfaceType = ReplaceGenericArgumentsAndConstraintsFromType(_interfaceType, _serviceInterceptorGenericParametersMap);
                _serviceInterceptorTypeBuilder.AddInterfaceImplementation(_implementedInterfaceType);
            }
            else
            {
                _implementedInterfaceType = _interfaceType;
                _serviceInterceptorTypeBuilder.AddInterfaceImplementation(_implementedInterfaceType);
            }

            _allImplementedInterfaces = GetAllImplementedInterfaces(_interfaceType);

            GenerateConstructors(_wrappedType, wrappedTypeGenericParametersMap);

            GenerateInterfaceMethods();

            GenerateInterfaceProperties();

            GenerateInterfaceEvents();

            return _serviceInterceptorTypeBuilder.CreateType()!;
        }

        private void GenerateConstructors(Type wrappedType, Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap)
        {
            var getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = _serviceInterceptorBaseType.GetConstructor(new[] { typeof(nint), typeof(Type), typeof(object?[]) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedTypeConstructors = wrappedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var gcHandleWrappedType = GCHandle.Alloc(wrappedType);
            var ptrHandleWrappedType = GCHandle.ToIntPtr(gcHandleWrappedType);
            var ptrHandleServiceInterceptorController = GCHandle.ToIntPtr(_serviceInterceptorHandle);

            foreach (var wrappedTypeConstructor in wrappedTypeConstructors)
            {
                var wrappedTypeConstructorParameters = wrappedTypeConstructor.GetParameters();
                var wrappedTypeConstructorParametersReplaced = wrappedTypeConstructorParameters.Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, wrappedTypeGenericParametersMap)).ToArray();
                var constructor = _serviceInterceptorTypeBuilder!.DefineConstructor(wrappedTypeConstructor.Attributes, wrappedTypeConstructor.CallingConvention, wrappedTypeConstructorParametersReplaced);
                for (int i = 0; i < wrappedTypeConstructorParameters.Length; i++)
                {
                    constructor.DefineParameter(i + 1, wrappedTypeConstructorParameters[i].Attributes, wrappedTypeConstructorParameters[i].Name);
                }
                var ilGenerator = constructor.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                ilGenerator.EmitWriteLine("Calling constructor for wrapped type");
                ilGenerator.Emit(OpCodes.Ldarg_0);
                if (IntPtr.Size == 4)
                    ilGenerator.Emit(OpCodes.Ldc_I4, ptrHandleServiceInterceptorController.ToInt32());
                else
                    ilGenerator.Emit(OpCodes.Ldc_I8, ptrHandleServiceInterceptorController.ToInt64());
                if (IntPtr.Size == 4)
                    ilGenerator.Emit(OpCodes.Ldc_I4, ptrHandleWrappedType.ToInt32());
                else
                    ilGenerator.Emit(OpCodes.Ldc_I8, ptrHandleWrappedType.ToInt64());
                ilGenerator.Emit(OpCodes.Ldobj, typeof(Type));
                if (!wrappedTypeConstructorParameters.Any())
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Call, baseConstructor);
                    ilGenerator.Emit(OpCodes.Nop);
                    ilGenerator.Emit(OpCodes.Nop);
                    ilGenerator.Emit(OpCodes.Ret);
                    continue;
                }

                ilGenerator.Emit(OpCodes.Ldc_I4, wrappedTypeConstructorParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc, localOjectParamList);

                for (short parameterIndex = 0; parameterIndex < wrappedTypeConstructorParameters.Length; parameterIndex++)
                {
                    var loadArgumentLabel = ilGenerator.DefineLabel();
                    var loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldtoken, wrappedTypeConstructorParametersReplaced[parameterIndex]);
                    ilGenerator.Emit(OpCodes.Call, getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Box, wrappedTypeConstructorParametersReplaced[parameterIndex]);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Stelem_Ref);

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                ilGenerator.Emit(OpCodes.Call, baseConstructor);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }
    }
}
