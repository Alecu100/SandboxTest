using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    public class ServiceInterceptorTypeBuilder
    {
        private static MethodInfo _getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
        private static MethodInfo _invokeMethod = typeof(ServiceInterceptor).GetMethod(ServiceInterceptor.InvokeMethodName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get current method");
        private static MethodInfo _getCurrentMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Could not get current method");
        private static MethodInfo _objectGetTypeMethod = typeof(ServiceInterceptor).GetType().GetMethod(nameof(GetType), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not get get type object method");
        private static MethodInfo _objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");

        private readonly Type _interfaceType;
        private readonly Type? _wrappedType;
        private readonly ServiceInterceptorController _controller;

        private TypeBuilder? _serviceInterceptorTypeBuilder;
        private Dictionary<Type, GeneraticParameterTypeWithInitialization>? _serviceInterceptorGenericParametersMap = null;
        private Dictionary<Type, GeneraticParameterTypeWithInitialization>? _wrappedTypeGenericParametersMap = null;
        private Type? _implementedInterfaceType;
        private List<Type>? _allImplementedInterfaces;
        private Type _serviceInterceptorBaseType;
        private List<MethodBuilder> _builtMethods;

        public ServiceInterceptorTypeBuilder(Type interfaceType, Type wrappedType, ServiceInterceptorController serviceInterceptorController) 
        {
            _interfaceType = interfaceType;
            _wrappedType = wrappedType;
            _controller = serviceInterceptorController;
        }

        public ServiceInterceptorTypeBuilder(Type interfaceType, ServiceInterceptorController serviceInterceptorController) 
        {
            _interfaceType = interfaceType;
            _controller = serviceInterceptorController;
        }

        public Type Build()
        {
            if (_wrappedType == null)
            {
                return CreateServiceInterceptorInterfaceWrapper();
            }
            else
            {
                return CreateServiceInterceptorClassWrapper();
            }
        }

        private Type CreateServiceInterceptorClassWrapper()
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
            _serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(_interfaceType.Name)}.{MakeSafeName(_wrappedType.Name)}.{guid}.dll");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run | AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            _serviceInterceptorTypeBuilder = moduleBuilder.DefineType(serviceInterceptorTypeName, TypeAttributes.Public | TypeAttributes.Class, _serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap = null;
            _builtMethods = new List<MethodBuilder>();

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

            var createdType = _serviceInterceptorTypeBuilder.CreateType();
            _controller.AddReference(createdType);
            return createdType;
        }

        private List<Type> GetAllImplementedInterfaces(Type implementedInterfaceType)
        {
            var allImplementedInterfaces = new List<Type>();
            foreach (var parentInterface in implementedInterfaceType.GetInterfaces())
            {
                allImplementedInterfaces.AddRange(GetAllImplementedInterfaces(parentInterface));
            }
            if (implementedInterfaceType.IsGenericType)
            {
                var genericTypeArguments = implementedInterfaceType.GetGenericArguments().Select(argument => ReplaceGenericArgumentsFromType(argument, _serviceInterceptorGenericParametersMap));
                var implementedInterfaceTypeProper = implementedInterfaceType.GetGenericTypeDefinition().MakeGenericType(genericTypeArguments.ToArray());
                allImplementedInterfaces.Add(implementedInterfaceTypeProper);
            }
            else
            {
                allImplementedInterfaces.Add(implementedInterfaceType);
            }
            return allImplementedInterfaces;
        }

        private Type CreateServiceInterceptorInterfaceWrapper()
        {
            var guid = Guid.NewGuid();
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(_interfaceType.Name)}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            _serviceInterceptorTypeBuilder = moduleBuilder.DefineType($"ServiceInterceptor-{MakeSafeName(_interfaceType.Name)}-{guid}", TypeAttributes.Public | TypeAttributes.Class, serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap = null;
            List<MethodBuilder> builtMethods = new List<MethodBuilder>();

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

            var createdType = _serviceInterceptorTypeBuilder.CreateType();
            _controller.AddReference(createdType);
            return createdType;
        }

        private static bool InterfacesAreEquivalent(Type interface1, Type interface2)
        {
            if (interface1 == interface2)
            {
                return true;
            }

            if (interface1.IsAssignableFrom(interface2))
            {
                return true;
            }

            if (interface2.IsAssignableFrom(interface1))
            {
                return true;
            }

            if (interface1.IsGenericType && !interface1.IsGenericTypeDefinition && interface1.GetGenericTypeDefinition() == interface2)
            {
                return true;
            }

            if (interface2.IsGenericType && !interface2.IsGenericTypeDefinition && interface1.GetGenericTypeDefinition() == interface1)
            {
                return true;
            }

            return false;
        }

        private void GenerateInterfaceEvents()
        {
            var interfaceEvents = _interfaceType.GetEvents(BindingFlags.Instance);

            foreach (var interfaceEvent in interfaceEvents)
            {
                if (interfaceEvent.EventHandlerType == null)
                {
                    continue;
                }
                var interfaceEventImplementationDelegateType = ReplaceGenericArgumentsFromType(interfaceEvent.EventHandlerType, _serviceInterceptorGenericParametersMap);
                var interfaceEventImplementation = _serviceInterceptorTypeBuilder!.DefineEvent(interfaceEvent.Name, interfaceEvent.Attributes, interfaceEventImplementationDelegateType);
                var removeEventMethod = _builtMethods.FirstOrDefault(method => method.Name == $"remove_{interfaceEvent.Name}");
                var addEventMethod = _builtMethods.FirstOrDefault(method => method.Name == $"add_{interfaceEvent.Name}");
                if (removeEventMethod != null)
                {
                    interfaceEventImplementation.SetRemoveOnMethod(removeEventMethod);
                }
                if (addEventMethod != null)
                {
                    interfaceEventImplementation.SetAddOnMethod(addEventMethod);
                }
            }
        }

        private void GenerateInterfaceProperties()
        {
            var interfaceProperties = GetAllInterfaceProperties(_interfaceType);

            foreach (var interfaceProperty in interfaceProperties)
            {
                var returnType = ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, _serviceInterceptorGenericParametersMap);
                var parameterTypes = interfaceProperty.GetIndexParameters().Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, _serviceInterceptorGenericParametersMap)).ToArray();
                var property = _serviceInterceptorTypeBuilder!.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, _serviceInterceptorGenericParametersMap), parameterTypes);
                var getMethodProperty = _builtMethods.FirstOrDefault(builtMethod => builtMethod.Name == $"get_{property.Name}");
                var setMethodProperty = _builtMethods.FirstOrDefault(builtMethod => builtMethod.Name == $"set_{property.Name}");
                if (getMethodProperty != null)
                {
                    property.SetGetMethod(getMethodProperty);
                }
                if (setMethodProperty != null)
                {
                    property.SetSetMethod(setMethodProperty);
                }
            }

        }

        private void GenerateInterfaceMethods()
        {
            var interfaceMethods = GetAllInterfaceMethods(_interfaceType);

            foreach (var interfaceMethod in interfaceMethods)
            {
                if (interfaceMethod == null || interfaceMethod.DeclaringType == null)
                {
                    continue;
                }
                var isExplicitMethodImplementation = false;
                MethodInfo? interfaceMethodToExplicitlyImplement = null;
                foreach (var otherInterfaceMethod in interfaceMethods)
                {
                    if (otherInterfaceMethod == interfaceMethod || otherInterfaceMethod.DeclaringType == null)
                    {
                        continue;
                    }
                    var otherInterfaceMethodParameters = otherInterfaceMethod.GetParameters();
                    var interfaceMethodParameters = interfaceMethod.GetParameters();
                    if (otherInterfaceMethodParameters.Length != interfaceMethodParameters.Length)
                    {
                        continue;
                    }
                    if (otherInterfaceMethod.Name != interfaceMethod.Name)
                    {
                        continue;
                    }
                    for (int parameterIndex = 0; parameterIndex < otherInterfaceMethodParameters.Length; parameterIndex++)
                    {
                        if (otherInterfaceMethodParameters[parameterIndex].ParameterType != interfaceMethodParameters[parameterIndex].ParameterType)
                        {
                            continue;
                        }
                        if (otherInterfaceMethod.DeclaringType.IsAssignableTo(interfaceMethod.DeclaringType))
                        {
                            isExplicitMethodImplementation = true;
                            if (otherInterfaceMethod.DeclaringType.IsGenericType)
                            {
                                var otherInterfaceMethodGenericArguments = otherInterfaceMethod.DeclaringType.GetGenericArguments().Select(arg => ReplaceGenericArgumentsFromType(arg, _serviceInterceptorGenericParametersMap)).ToArray();
                                var otherInterfaceGenericImplementation = otherInterfaceMethod.DeclaringType.GetGenericTypeDefinition().MakeGenericType(otherInterfaceMethodGenericArguments);
                                interfaceMethodToExplicitlyImplement = MethodBase.GetMethodFromHandle(otherInterfaceMethod.MethodHandle, otherInterfaceGenericImplementation.TypeHandle) as MethodInfo;
                            }
                            else
                            {
                                interfaceMethodToExplicitlyImplement = interfaceMethod;
                            }
                            break;
                        }
                    }
                }
                if (isExplicitMethodImplementation && (interfaceMethodToExplicitlyImplement == null || interfaceMethodToExplicitlyImplement.DeclaringType == null))
                {
                    throw new InvalidOperationException("Could not find method to explicitly implement");
                }
                var interfaceMethodImplementationName = isExplicitMethodImplementation ? $"{interfaceMethodToExplicitlyImplement!.DeclaringType!.Name}.{interfaceMethod.Name}" : interfaceMethod.Name;
                var interfaceMethodImplementation = _serviceInterceptorTypeBuilder!.DefineMethod(interfaceMethodImplementationName, interfaceMethod.Attributes & ~MethodAttributes.Abstract, interfaceMethod.CallingConvention);
                _builtMethods.Add(interfaceMethodImplementation);
                var interfaceMethodArgumentsType = new List<Type>();
                var interfaceMethodGenericArguments = interfaceMethod.GetGenericArguments();
                var interfaceMethodGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                if (_serviceInterceptorGenericParametersMap != null && _serviceInterceptorGenericParametersMap.Any())
                {
                    foreach (var parameterMap in _serviceInterceptorGenericParametersMap)
                    {
                        interfaceMethodGenericParametersMap[parameterMap.Key] = parameterMap.Value;
                    }
                }
                if (interfaceMethodGenericArguments.Any())
                {
                    var genericMethodParameters = interfaceMethodImplementation.DefineGenericParameters(interfaceMethodGenericArguments.Select(arg => arg.Name).ToArray());
                    for (int i = 0; i < genericMethodParameters.Length; i++)
                    {
                        interfaceMethodGenericParametersMap[interfaceMethodGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = genericMethodParameters[i] };
                    }
                    foreach (var interfaceMethodGenericArgument in interfaceMethodGenericArguments)
                    {
                        foreach (var contraint in interfaceMethodGenericArgument.GetGenericParameterConstraints())
                        {
                            var replacedConstraint = ReplaceGenericArgumentsAndConstraintsFromType(contraint, interfaceMethodGenericParametersMap);
                            if (replacedConstraint.IsInterface)
                            {
                                interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder.SetInterfaceConstraints(replacedConstraint);
                                continue;
                            }
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder.SetBaseTypeConstraint(replacedConstraint);
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder.SetGenericParameterAttributes(contraint.GenericParameterAttributes & ~GenericParameterAttributes.Covariant & ~GenericParameterAttributes.Contravariant);
                        }
                    }
                }

                var interfaceMethodImplementationParameters = interfaceMethod.GetParameters().Select(x => ReplaceGenericArgumentsFromType(x.ParameterType, _serviceInterceptorGenericParametersMap)).ToArray();
                var interfaceMethodReturnType = ReplaceGenericArgumentsFromType(interfaceMethod.ReturnType ?? typeof(void), _serviceInterceptorGenericParametersMap);
                interfaceMethodImplementation.SetParameters(interfaceMethodImplementationParameters);
                interfaceMethodImplementation.SetReturnType(interfaceMethodReturnType);

                if (isExplicitMethodImplementation)
                {
                    _serviceInterceptorTypeBuilder.DefineMethodOverride(interfaceMethodImplementation, interfaceMethodToExplicitlyImplement!);
                }

                var ilGenerator = interfaceMethodImplementation.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                var localMethodInfo = ilGenerator.DeclareLocal(typeof(MethodInfo));
                var localInvokeReturnedObject = ilGenerator.DeclareLocal(typeof(object));
                var localUnboxedInvokeReturnedObject = ilGenerator.DeclareLocal(interfaceMethodReturnType.GetType());
                ilGenerator.EmitWriteLine("Calling generated interface method");
                ilGenerator.Emit(OpCodes.Call, _getCurrentMethod);
                ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
                ilGenerator.Emit(OpCodes.Stloc, localMethodInfo);

                ilGenerator.Emit(OpCodes.Ldc_I4, interfaceMethodImplementationParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc, localOjectParamList);

                for (short parameterIndex = 0; parameterIndex < interfaceMethodImplementationParameters.Length; parameterIndex++)
                {
                    var loadArgumentLabel = ilGenerator.DefineLabel();
                    var loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldtoken, interfaceMethodImplementationParameters[parameterIndex]);
                    ilGenerator.Emit(OpCodes.Call, _getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, _objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.EmitWriteLine("Detected value type, boxing it");
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Box, interfaceMethodImplementationParameters[parameterIndex]);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem_Ref);
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.EmitWriteLine("Detected reference type,putting it directly in array");
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldc_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Stelem_Ref);

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldloc, localMethodInfo);
                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                ilGenerator.Emit(OpCodes.Callvirt, _invokeMethod);
                var returnLabel = ilGenerator.DefineLabel();
                if (interfaceMethod.ReturnType == null || interfaceMethod.ReturnType == typeof(void))
                {
                    ilGenerator.Emit(OpCodes.Pop);
                    ilGenerator.Emit(OpCodes.Br, returnLabel);
                }
                else
                {
                    var activatorCreateTypeMethod = typeof(Activator).GetMethod(nameof(Activator.CreateInstance), BindingFlags.Static | BindingFlags.Public, new Type[] { typeof(Type) })
                        ?? throw new InvalidOperationException("Could not get activator create instance method");
                    ilGenerator.Emit(OpCodes.Stloc, localInvokeReturnedObject);
                    var loadInvokeReturnDirectlyLabel = ilGenerator.DefineLabel();
                    var loadDefaultValueTypeLabel = ilGenerator.DefineLabel();
                    ilGenerator.Emit(OpCodes.Ldtoken, interfaceMethodReturnType);
                    ilGenerator.Emit(OpCodes.Call, _getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, _objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadInvokeReturnDirectlyLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Brfalse, loadDefaultValueTypeLabel);
                    ilGenerator.EmitWriteLine("Loading unboxed returned object from invoke");
                    ilGenerator.EmitWriteLine(localInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Ldloc, localInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Unbox_Any, interfaceMethodReturnType);
                    ilGenerator.Emit(OpCodes.Stloc, localUnboxedInvokeReturnedObject);
                    ilGenerator.EmitWriteLine("Unboxed returned invoke object:");
                    ilGenerator.Emit(OpCodes.Ldloc, localUnboxedInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Br, returnLabel);
                    ilGenerator.MarkLabel(loadInvokeReturnDirectlyLabel);
                    ilGenerator.EmitWriteLine("Loading returned object from invoke directly");
                    ilGenerator.EmitWriteLine(localInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Ldloc, localInvokeReturnedObject);
                    ilGenerator.Emit(OpCodes.Br, returnLabel);
                    ilGenerator.MarkLabel(loadDefaultValueTypeLabel);
                    ilGenerator.EmitWriteLine("Loading default value type because invoke returned null");
                    ilGenerator.Emit(OpCodes.Ldtoken, interfaceMethodReturnType);
                    ilGenerator.Emit(OpCodes.Call, _getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Call, activatorCreateTypeMethod);
                    ilGenerator.Emit(OpCodes.Unbox, interfaceMethodReturnType);
                    ilGenerator.Emit(OpCodes.Br, returnLabel);
                }

                ilGenerator.MarkLabel(returnLabel);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private static string MakeSafeName(string name)
        {
            return name.Replace("'", "").Replace("`", "").Replace("-", "_").Replace(",", "").Replace(";", "");
        }

        private static List<Type> GetAllInterfacesImplementedByType(Type type)
        {
            var allInterfaces = type.GetInterfaces().ToList();
            foreach (var inf in allInterfaces.ToArray())
            {
                if (inf != null)
                {
                    allInterfaces.AddRange(GetAllInterfacesImplementedByType(inf));
                }
            }

            return allInterfaces;
        }

        private static List<MethodInfo> GetAllInterfaceMethods(Type interfaceType)
        {
            var interfaceMethods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceMethods.AddRange(GetAllInterfaceMethods(implementedInterface));
            }

            return interfaceMethods;
        }

        private static List<PropertyInfo> GetAllInterfaceProperties(Type interfaceType)
        {
            var interfaceProperties = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            interfaceProperties.AddRange(interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic));
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceProperties.AddRange(GetAllInterfaceProperties(implementedInterface));
            }

            return interfaceProperties;
        }

        private void GenerateConstructor()
        {
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = _serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController), typeof(object) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var constructor = _serviceInterceptorTypeBuilder!.DefineConstructor(baseConstructor.Attributes, baseConstructor.CallingConvention,
                baseConstructor.GetParameters().Select(param => param.ParameterType).ToArray());
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.EmitWriteLine("Calling generated constructor for interface type");
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Call, baseConstructor);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Nop);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private void GenerateConstructors(Type wrappedType, Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap)
        {
            var getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = _serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController), typeof(Type), typeof(object?[]) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedTypeConstructors = wrappedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var gcHandleWrappedType = GCHandle.Alloc(wrappedType);
            var ptrHandleWrappedType = GCHandle.ToIntPtr(gcHandleWrappedType);

            _controller.AddReference(wrappedType);

            foreach (var wrappedTypeConstructor in wrappedTypeConstructors)
            {
                var wrappedTypeConstructorParameters = wrappedTypeConstructor.GetParameters().Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, wrappedTypeGenericParametersMap)).ToArray();
                var constructorParameters = new[] { serviceInterceptorControllerType }.Concat(wrappedTypeConstructorParameters).ToArray();
                var constructor = _serviceInterceptorTypeBuilder!.DefineConstructor(wrappedTypeConstructor.Attributes, wrappedTypeConstructor.CallingConvention, constructorParameters);
                var ilGenerator = constructor.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                ilGenerator.EmitWriteLine("Calling constructor for wrapped type");
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                if (nint.Size == 4)
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

                    ilGenerator.Emit(OpCodes.Ldtoken, wrappedTypeConstructorParameters[parameterIndex]);
                    ilGenerator.Emit(OpCodes.Call, getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Box, wrappedTypeConstructorParameters[parameterIndex]);
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

        private static Type ReplaceGenericArgumentsAndConstraintsFromType(Type type, Dictionary<Type, GeneraticParameterTypeWithInitialization> genericParametersMap)
        {
            if (!type.IsGenericTypeDefinition)
            {
                if (genericParametersMap.ContainsKey(type))
                {
                    if (!genericParametersMap[type].IsInitialized)
                    {
                        foreach (var genericConstraint in type.GetGenericParameterConstraints())
                        {
                            var replacedGenericConstraint = ReplaceGenericArgumentsAndConstraintsFromType(genericConstraint, genericParametersMap);
                            if (replacedGenericConstraint.IsInterface)
                            {
                                genericParametersMap[type].GenericTypeParameterBuilder.SetInterfaceConstraints(replacedGenericConstraint);
                                continue;
                            }
                            genericParametersMap[type].GenericTypeParameterBuilder.SetBaseTypeConstraint(replacedGenericConstraint);
                            genericParametersMap[type].GenericTypeParameterBuilder.SetGenericParameterAttributes(type.GenericParameterAttributes & ~GenericParameterAttributes.Covariant & ~GenericParameterAttributes.Contravariant);
                        }
                    }

                    return genericParametersMap[type].GenericTypeParameterBuilder;
                }
                return type;
            }
            var replacedGenericParameters = new List<Type>();
            foreach (var genericTypeParameter in type.GetGenericArguments())
            {
                var processedGenericParameter = ReplaceGenericArgumentsAndConstraintsFromType(genericTypeParameter, genericParametersMap);
                replacedGenericParameters.Add(processedGenericParameter);
            }
            return type.MakeGenericType(replacedGenericParameters.ToArray());
        }

        private static Type ReplaceGenericArgumentsFromType(Type type, Dictionary<Type, GeneraticParameterTypeWithInitialization>? genericParametersMap)
        {
            if (genericParametersMap == null)
            {
                return type;
            }

            if (genericParametersMap.ContainsKey(type))
            {
                return genericParametersMap[type].GenericTypeParameterBuilder;
            }

            if (type.IsGenericType)
            {
                var typeDefinition = type.GetGenericTypeDefinition();
                var typeGenericArguments = type.GetGenericArguments();
                var newTypeArguments = typeGenericArguments.Select(arg => ReplaceGenericArgumentsFromType(arg, genericParametersMap)).ToArray();
                return typeDefinition.MakeGenericType(newTypeArguments);
            }

            return type;
        }

        private class GeneraticParameterTypeWithInitialization
        {
            required public GenericTypeParameterBuilder GenericTypeParameterBuilder { get; set; }

            public bool IsInitialized { get; set; } = false;
        }
    }
}
