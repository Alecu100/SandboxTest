using System.Reflection;
using System.Reflection.Emit;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    public class ServiceInterceptor : DispatchProxy
    {
        private object? _wrappedInstance;
        private ServiceInterceptorController _controller;
        private List<Type> _interfaceType;

        public ServiceInterceptor(ServiceInterceptorController controller)
        {
            _controller = controller;
            _interfaceType = GetType().GetInterfaces().ToList();
        }

        public ServiceInterceptor(ServiceInterceptorController controller, object wrappedInstance)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
            _interfaceType = GetType().GetInterfaces().ToList();
        }

        public static Type CreateServiceInterceptorTypeWrapper(Type interfaceType, Type wrappedType)
        {
            if (!GetAllInterfacesImplementedByType(wrappedType).Any(wrappedInterfaceType => InterfacesAreEquivalent(wrappedInterfaceType, interfaceType)))
            {
                throw new InvalidOperationException($"Wrapped type {wrappedType.FullName} must implement interface type {interfaceType.FullName}");
            }
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(interfaceType.Name)}.{MakeSafeName(wrappedType.Name)}.dll");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            var serviceInterceptorTypeBuilder = moduleBuilder.DefineType($"ServiceInterceptor{MakeSafeName(interfaceType.Name)}{MakeSafeName(wrappedType.Name)}", TypeAttributes.Public, serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap = null;
            List<MethodBuilder> builtMethods = new List<MethodBuilder>();

            if (interfaceType.IsGenericTypeDefinition)
            {
                if (!wrappedType.IsGenericTypeDefinition)
                {
                    throw new InvalidOperationException("Wrapped type is not an open generic type whereas the interface is");
                }
                var interfaceGenericArguments = interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                serviceInterceptorGenericParametersMap = new Dictionary<Type, GenericTypeParameterBuilder>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = serviceInterceptorGenericParameters[i];
                }

                interfaceType = ReplaceGenericArgumentsAndConstraintsFromGenericType(interfaceType, serviceInterceptorGenericParametersMap);
                wrappedType = wrappedType.MakeGenericType(serviceInterceptorGenericParameters.Select(param => param.AsType()).ToArray());
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);
            }
            else
            {
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);
            }

            GenerateConstructors(wrappedType, serviceInterceptorBaseType, serviceInterceptorTypeBuilder);

            GenerateInterfaceMethods(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceProperties(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap);

            return serviceInterceptorTypeBuilder.CreateType();
        }

        public static Type CreateServiceInterceptorTypeWrapper(Type interfaceType)
        {
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(interfaceType.Name)}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            var serviceInterceptorTypeBuilder = moduleBuilder.DefineType($"ServiceInterceptor{MakeSafeName(interfaceType.Name)}", TypeAttributes.Public | TypeAttributes.Class, serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap = null;
            List<MethodBuilder> builtMethods = new List<MethodBuilder>();

            if (interfaceType.IsGenericTypeDefinition)
            {
                var interfaceGenericArguments = interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                serviceInterceptorGenericParametersMap = new Dictionary<Type, GenericTypeParameterBuilder>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = serviceInterceptorGenericParameters[i];
                }

                interfaceType = ReplaceGenericArgumentsAndConstraintsFromGenericType(interfaceType, serviceInterceptorGenericParametersMap);
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType.MakeGenericType(serviceInterceptorGenericParameters.Select(param => param.AsType()).ToArray()));
            }
            else
            {
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);
            }

            GenerateConstructor(serviceInterceptorBaseType, serviceInterceptorTypeBuilder);

            GenerateInterfaceMethods(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceProperties(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap);

            return serviceInterceptorTypeBuilder.CreateType();
        }

        private static bool InterfacesAreEquivalent(Type interface1, Type interface2)
        {
            if (interface1 == interface2)
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

        private static void GenerateInterfaceEvents(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap)
        {
            var interfaceEvents = interfaceType.GetEvents(BindingFlags.Instance);

            foreach (var interfaceEvent in interfaceEvents)
            {
                serviceInterceptorTypeBuilder.DefineEvent(interfaceEvent.Name, interfaceEvent.Attributes, interfaceEvent.EventHandlerType ?? throw new Exception($"Failed to create event {interfaceEvent.Name} for interface {interfaceType.Name}"));
            }
        }

        private static void GenerateInterfaceProperties(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var interfaceProperties = GetAllInterfaceProperties(interfaceType);

            foreach (var interfaceProperty in interfaceProperties) 
            {
                var property = serviceInterceptorTypeBuilder.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, interfaceProperty.PropertyType, Type.EmptyTypes);
                var getMethodProperty = builtMethods.FirstOrDefault(builtMethod => builtMethod.Name == $"get_{property.Name}");
                var setMethodProperty = builtMethods.FirstOrDefault(builtMethod => builtMethod.Name == $"set_{property.Name}");
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

        private static void GenerateInterfaceMethods(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var invokeMethod = typeof(ServiceInterceptor).GetMethod(nameof(Invoke), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get current method"); 
            var getCurrentMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Could not get current method");
            var interfaceMethods = GetAllInterfaceMethods(interfaceType);
            var objectGetTypeMethod = typeof(ServiceInterceptor).GetType().GetMethod(nameof(GetType), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not get get type object method");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");

            foreach (var interfaceMethod in interfaceMethods)
            {
                var interfaceMethodTypeBuilder = serviceInterceptorTypeBuilder.DefineMethod(interfaceMethod.Name, (interfaceMethod.Attributes) & ~(MethodAttributes.Abstract), interfaceMethod.CallingConvention);
                builtMethods.Add(interfaceMethodTypeBuilder);
                var interfaceMethodArgumentsType = new List<Type>();
                var interfaceMethodGenericArguments = interfaceMethod.GetGenericArguments();
                var interfaceMethodGenericParametersMap = new Dictionary<Type, GenericTypeParameterBuilder>();
                if (serviceInterceptorGenericParametersMap != null && serviceInterceptorGenericParametersMap.Any())
                {
                    foreach (var parameterMap in serviceInterceptorGenericParametersMap)
                    {
                        interfaceMethodGenericParametersMap[parameterMap.Key] = parameterMap.Value;
                    }
                }
                if (interfaceMethodGenericArguments.Any())
                {
                    var genericMethodParameters = interfaceMethodTypeBuilder.DefineGenericParameters(interfaceMethodGenericArguments.Select(arg => arg.Name).ToArray());
                    for (int i = 0; i < genericMethodParameters.Length; i++)
                    {
                        interfaceMethodGenericParametersMap[interfaceMethodGenericArguments[i]] = genericMethodParameters[i];
                    }
                    foreach (var interfaceMethodGenericArgument in interfaceMethodGenericArguments)
                    {
                        foreach (var contraint in interfaceMethodGenericArgument.GetGenericParameterConstraints())
                        {
                            var replacedConstraint = ReplaceGenericArgumentsAndConstraintsFromGenericType(contraint, interfaceMethodGenericParametersMap);
                            if (replacedConstraint.IsInterface)
                            {
                                interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].SetInterfaceConstraints(replacedConstraint);
                                continue;
                            }
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].SetBaseTypeConstraint(replacedConstraint);
                        }
                    }
                }
                interfaceMethodTypeBuilder.SetParameters(interfaceMethod.GetParameters().Select(x => x.ParameterType).ToArray());
                interfaceMethodTypeBuilder.SetReturnType(interfaceMethod.ReturnType);

                var interfaceParameters = interfaceMethod.GetParameters();
                var ilGenerator = interfaceMethodTypeBuilder.GetILGenerator();
                ilGenerator.Emit(OpCodes.Call, getCurrentMethod);
                ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
                ilGenerator.Emit(OpCodes.Stloc_2);
                if (!interfaceMethod.GetParameters().Any())
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldloc_2);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Callvirt, invokeMethod);
                    ilGenerator.Emit(OpCodes.Ret);
                    continue;
                }

                ilGenerator.Emit(OpCodes.Ldc_I4, interfaceParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc_0);
                var loadArgumentLabel = ilGenerator.DefineLabel();
                var loadNextArgumentLabel = ilGenerator.DefineLabel();
                if (interfaceParameters.Length >= 1) 
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc_1);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 0);
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 0);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                }

                ilGenerator.MarkLabel(loadNextArgumentLabel);

                if (interfaceParameters.Length >= 2)
                {
                    loadArgumentLabel = ilGenerator.DefineLabel();
                    loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc_1);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 1);
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 1);
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                if (interfaceParameters.Length >= 3)
                {
                    loadArgumentLabel = ilGenerator.DefineLabel();
                    loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc_1);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 2);
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 2);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                for (short parameterIndex = 3; parameterIndex < interfaceParameters.Length; parameterIndex++)
                {
                    loadArgumentLabel = ilGenerator.DefineLabel();
                    loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc_1);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldloc_1);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc_0);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldloc_2);
                ilGenerator.Emit(OpCodes.Ldloc_1);
                ilGenerator.Emit(OpCodes.Callvirt, invokeMethod);
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
            foreach (var inf in allInterfaces) 
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
            var interfaceProperties = interfaceType.GetProperties(BindingFlags.Instance).ToList();
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceProperties.AddRange(GetAllInterfaceProperties(implementedInterface));
            }

            return interfaceProperties;
        }

        private static void GenerateConstructor(Type serviceInterceptorBaseType, TypeBuilder serviceInterceptorTypeBuilder)
        {
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController), typeof(object) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedInstanceField = serviceInterceptorBaseType.GetField(nameof(_wrappedInstance), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get wrapped instance field");
            var constructor = serviceInterceptorTypeBuilder.DefineConstructor(baseConstructor.Attributes, baseConstructor.CallingConvention,
                baseConstructor.GetParameters().Select(param => param.ParameterType).ToArray());
            var ilGenerator = constructor.GetILGenerator();
            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Ldarg_2);
            ilGenerator.Emit(OpCodes.Call, baseConstructor);
            ilGenerator.Emit(OpCodes.Ret);
        }

        private static void GenerateConstructors(Type wrappedType, Type serviceInterceptorBaseType, TypeBuilder serviceInterceptorTypeBuilder)
        {
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedTypeConstructors = wrappedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var wrappedInstanceField = serviceInterceptorBaseType.GetField(nameof(_wrappedInstance), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get wrapped instance field");
          
            foreach (var wrappedTypeConstructor in wrappedTypeConstructors)
            {
                var wrappedTypeConstructorParameters = wrappedTypeConstructor.GetParameters();
                var constructor = serviceInterceptorTypeBuilder.DefineConstructor(wrappedTypeConstructor.Attributes, wrappedTypeConstructor.CallingConvention,
                    new[] { serviceInterceptorControllerType }.Concat(wrappedTypeConstructorParameters.Select(param => param.ParameterType)).ToArray());
                var ilGenerator = constructor.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Call, baseConstructor);
                if (wrappedTypeConstructorParameters.Length >= 2)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                }
                if (wrappedTypeConstructorParameters.Length >= 3)
                {
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                }
                for (short constructorParamIndex = 4; constructorParamIndex < wrappedTypeConstructorParameters.Length + 2; constructorParamIndex++)
                {
                    ilGenerator.Emit(OpCodes.Ldarg, constructorParamIndex);
                }
                ilGenerator.Emit(OpCodes.Newobj, wrappedTypeConstructor);
                ilGenerator.Emit(OpCodes.Stloc_0);
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldloc_0);
                ilGenerator.Emit(OpCodes.Stfld, wrappedInstanceField);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private static Type ReplaceGenericArgumentsAndConstraintsFromGenericType(Type type, Dictionary<Type, GenericTypeParameterBuilder> genericParametersMap)
        {
            if (!type.IsGenericTypeDefinition)
            {
                if (genericParametersMap.ContainsKey(type))
                {
                    foreach (var genericConstraint in type.GetGenericParameterConstraints())
                    {
                        var replacedGenericConstraint = ReplaceGenericArgumentsAndConstraintsFromGenericType(genericConstraint, genericParametersMap);
                        if (genericParametersMap[type].GetGenericParameterConstraints().Any(constraint => replacedGenericConstraint == constraint))
                        {
                            continue;
                        }
                        genericParametersMap[type].SetGenericParameterAttributes(type.GenericParameterAttributes);
                        if (replacedGenericConstraint.IsInterface)
                        {
                            genericParametersMap[type].SetInterfaceConstraints(replacedGenericConstraint);
                            continue;
                        }
                        genericParametersMap[type].SetBaseTypeConstraint(replacedGenericConstraint);
                    }
                    return genericParametersMap[type];
                }
                return type;
            }
            var replacedGenericParameters = new List<Type>();
            foreach (var genericTypeParameter in type.GetGenericArguments())
            {
                var processedGenericParameter = ReplaceGenericArgumentsAndConstraintsFromGenericType(genericTypeParameter, genericParametersMap);
                replacedGenericParameters.Add(processedGenericParameter);
            }
            return type.MakeGenericType(replacedGenericParameters.ToArray());
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (_interfaceType == null || _controller == null || _wrappedInstance == null)
            {
                throw new InvalidOperationException("Proxy interceptor not initialized");
            }
            if (targetMethod == null)
            {
                return null;
            }
            foreach (var interfaceType in _interfaceType)
            {
                if (_controller.ProxyWrapperActions.ContainsKey(interfaceType) && _controller.ProxyWrapperActions[interfaceType].ContainsKey(targetMethod))
                {
                    var methodActions = _controller.ProxyWrapperActions[interfaceType][targetMethod];
                    for (int actionIndex = methodActions.Count - 1; actionIndex >= 0; actionIndex--)
                    {
                        ServiceInterceptorAction? action = methodActions[actionIndex];
                        if (action.ArgumentsMatcher(args))
                        {
                            if (targetMethod.ReturnParameter.ParameterType != typeof(void))
                            {
                                if (action.CallReplaceFunc == null)
                                {
                                    return targetMethod.Invoke(_wrappedInstance, args);
                                }
                                return action.CallReplaceFunc(_wrappedInstance, args);
                            }
                            if (action.CallReplaceAction == null)
                            {
                                targetMethod.Invoke(_wrappedInstance, args);
                                return null;
                            }
                            action.CallReplaceAction(_wrappedInstance, args);
                        }
                    }
                }
            }

            return targetMethod.Invoke(_wrappedInstance, args);
        }

        public void Initialize(ServiceInterceptorController controller, object? wrappedInstance, Type interfaceType)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
        }
    }
}
