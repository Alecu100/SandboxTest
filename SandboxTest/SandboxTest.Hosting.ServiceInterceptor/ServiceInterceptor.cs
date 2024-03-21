using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.ProxyInterceptor
{
    public class ServiceInterceptor : DispatchProxy
    {
        protected object? _wrappedInstance;
        protected ServiceInterceptorController _controller;
        protected List<Type> _interfaceType;

        public ServiceInterceptor(ServiceInterceptorController controller, Type wrappedType, object?[]? arguments)
        {
            _controller = controller;
            _interfaceType = GetType().GetInterfaces().ToList();
            if (wrappedType.IsGenericTypeDefinition)
            {
                if (!GetType().IsGenericType)
                {
                    throw new InvalidOperationException("Non generic service interceptor contains generic wrapped type");
                }
                var actualType = wrappedType.GetType().MakeGenericType(GetType().GetGenericArguments());
                _wrappedInstance = Activator.CreateInstance(actualType, arguments);
            }
            else
            {
                _wrappedInstance = Activator.CreateInstance(wrappedType, arguments);
            }
        }

        public ServiceInterceptor(ServiceInterceptorController controller, object wrappedInstance)
        {
            _wrappedInstance = wrappedInstance;
            _controller = controller;
            _interfaceType = GetType().GetInterfaces().ToList();
        }

        public static Type CreateServiceInterceptorClassWrapper(Type interfaceType, Type wrappedType, ServiceInterceptorController serviceInterceptorController)
        {
            if (!GetAllInterfacesImplementedByType(wrappedType).Any(wrappedInterfaceType => InterfacesAreEquivalent(wrappedInterfaceType, interfaceType)))
            {
                throw new InvalidOperationException($"Wrapped type {wrappedType.FullName} must implement interface type {interfaceType.FullName}");
            }
            var guid = Guid.NewGuid();
            var serviceInterceptorTypeName = $"ServiceInterceptor-{MakeSafeName(interfaceType.Name)}-{MakeSafeName(wrappedType.Name)}-{guid}";
            if (interfaceType.IsGenericTypeDefinition)
            {
                serviceInterceptorTypeName = $"ServiceInterceptor-Generic-{MakeSafeName(interfaceType.Name)}-{MakeSafeName(wrappedType.Name)}-{guid}";
            }
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(interfaceType.Name)}.{MakeSafeName(wrappedType.Name)}.{guid}.dll");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run | AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            var serviceInterceptorTypeBuilder = moduleBuilder.DefineType(serviceInterceptorTypeName, TypeAttributes.Public | TypeAttributes.Class, serviceInterceptorBaseType);
            GenericTypeParameterBuilder[]? serviceInterceptorGenericParameters = null;
            Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap = null;
            Dictionary<Type, GenericTypeParameterBuilder>? wrappedTypeGenericParametersMap = null;
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
                wrappedTypeGenericParametersMap = new Dictionary<Type, GenericTypeParameterBuilder>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = serviceInterceptorGenericParameters[i];
                }
                var wrappedTypeGenericArguments = wrappedType.GetGenericArguments();
                for (int i = 0; i < wrappedTypeGenericArguments.Length; i++)
                {
                    wrappedTypeGenericParametersMap[wrappedTypeGenericArguments[i]] = serviceInterceptorGenericParameters[i];
                }
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(ReplaceGenericArgumentsAndConstraintsFromType(interfaceType, serviceInterceptorGenericParametersMap));
            }
            else
            {
                serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);
            }

            GenerateConstructors(wrappedType, serviceInterceptorBaseType, serviceInterceptorTypeBuilder, wrappedTypeGenericParametersMap, serviceInterceptorController);

            GenerateInterfaceMethods(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceProperties(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap);

            var createdType = serviceInterceptorTypeBuilder.CreateType();
            serviceInterceptorController.AddReference(createdType);
            return createdType;
        }

        public static Type CreateServiceInterceptorInterfaceWrapper(Type interfaceType, ServiceInterceptorController serviceInterceptorController)
        {
            var guid = Guid.NewGuid();
            var serviceInterceptorBaseType = typeof(ServiceInterceptor);
            var assemblyName = new AssemblyName($"ServiceInterceptorProxyAssembly.{MakeSafeName(interfaceType.Name)}");
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? throw new InvalidOperationException("Could not create assembly name"));
            var serviceInterceptorTypeBuilder = moduleBuilder.DefineType($"ServiceInterceptor-{MakeSafeName(interfaceType.Name)}-{guid}", TypeAttributes.Public | TypeAttributes.Class, serviceInterceptorBaseType);
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

                interfaceType = ReplaceGenericArgumentsAndConstraintsFromType(interfaceType, serviceInterceptorGenericParametersMap);
            }

            serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);

            GenerateConstructor(serviceInterceptorBaseType, serviceInterceptorTypeBuilder);

            GenerateInterfaceMethods(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceProperties(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap);

            var createdType =  serviceInterceptorTypeBuilder.CreateType();
            serviceInterceptorController.AddReference(createdType);
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

        private static void GenerateInterfaceEvents(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap)
        {
            var interfaceEvents = interfaceType.GetEvents(BindingFlags.Instance);

            foreach (var interfaceEvent in interfaceEvents)
            {
                var e = serviceInterceptorTypeBuilder.DefineEvent(interfaceEvent.Name, interfaceEvent.Attributes, interfaceEvent.EventHandlerType ?? throw new Exception($"Failed to create event {interfaceEvent.Name} for interface {interfaceType.Name}"));
            }
        }

        private static void GenerateInterfaceProperties(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var interfaceProperties = GetAllInterfaceProperties(interfaceType);

            foreach (var interfaceProperty in interfaceProperties) 
            {
                var property = serviceInterceptorTypeBuilder.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, serviceInterceptorGenericParametersMap), Type.EmptyTypes);
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
                            var replacedConstraint = ReplaceGenericArgumentsAndConstraintsFromType(contraint, interfaceMethodGenericParametersMap);
                            if (replacedConstraint.IsInterface)
                            {
                                interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].SetInterfaceConstraints(replacedConstraint);
                                continue;
                            }
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].SetBaseTypeConstraint(replacedConstraint);
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].SetGenericParameterAttributes(contraint.GenericParameterAttributes & ~(GenericParameterAttributes.Covariant) & ~(GenericParameterAttributes.Contravariant));
                        }
                    }
                }
                var interfaceParameters = interfaceMethod.GetParameters();
                interfaceMethodTypeBuilder.SetParameters(interfaceParameters.Select(x => ReplaceGenericArgumentsFromType(x.ParameterType, serviceInterceptorGenericParametersMap)).ToArray());
                interfaceMethodTypeBuilder.SetReturnType(ReplaceGenericArgumentsFromType(interfaceMethod.ReturnType, serviceInterceptorGenericParametersMap));

                var ilGenerator = interfaceMethodTypeBuilder.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                var localMethodInfo = ilGenerator.DeclareLocal(typeof(MethodInfo));
                ilGenerator.EmitWriteLine("Calling generated interface method");
                ilGenerator.Emit(OpCodes.Call, getCurrentMethod);
                ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
                ilGenerator.Emit(OpCodes.Stloc, localMethodInfo);
                if (!interfaceMethod.GetParameters().Any())
                {
                    ilGenerator.Emit(OpCodes.Ldarg_0);
                    ilGenerator.Emit(OpCodes.Ldloc, localMethodInfo);
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Callvirt, invokeMethod);
                    ilGenerator.Emit(OpCodes.Ret);
                    continue;
                }

                ilGenerator.Emit(OpCodes.Ldc_I4, interfaceParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc, localOjectParamList);
                var loadArgumentLabel = ilGenerator.DefineLabel();
                var loadNextArgumentLabel = ilGenerator.DefineLabel();
                if (interfaceParameters.Length >= 1) 
                {
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Ldc_I4_0);
                    ilGenerator.Emit(OpCodes.Beq, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_1);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 0);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
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
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_2);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 1);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
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
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 2);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
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
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 1));
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldloc, localMethodInfo);
                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
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
            var constructor = serviceInterceptorTypeBuilder.DefineConstructor(baseConstructor.Attributes, baseConstructor.CallingConvention,
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

        private static void GenerateConstructors(Type wrappedType, Type serviceInterceptorBaseType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GenericTypeParameterBuilder>? wrappedTypeGenericParametersMap, ServiceInterceptorController controller)
        {
            var objectGetTypeMethod = typeof(ServiceInterceptor).GetType().GetMethod(nameof(GetType), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not get get type object method");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController), typeof(Type), typeof(object?[]) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedTypeConstructors = wrappedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var gcHandleWrappedType = GCHandle.Alloc(wrappedType);
            var ptrHandleWrappedType = GCHandle.ToIntPtr(gcHandleWrappedType);

            controller.AddReference(wrappedType);

            foreach (var wrappedTypeConstructor in wrappedTypeConstructors)
            {
                var wrappedTypeConstructorParameters = wrappedTypeConstructor.GetParameters();
                var constructor = serviceInterceptorTypeBuilder.DefineConstructor(wrappedTypeConstructor.Attributes, wrappedTypeConstructor.CallingConvention,
                    new[] { serviceInterceptorControllerType }.Concat(wrappedTypeConstructorParameters.Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, wrappedTypeGenericParametersMap))).ToArray());
                var ilGenerator = constructor.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                ilGenerator.EmitWriteLine("Calling constructor for wrapped type");
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
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
                }

                ilGenerator.Emit(OpCodes.Ldc_I4, wrappedTypeConstructorParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc, localOjectParamList);
                var loadArgumentLabel = ilGenerator.DefineLabel();
                var loadNextArgumentLabel = ilGenerator.DefineLabel();

                ilGenerator.Emit(OpCodes.Ldarg_2);
                ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                ilGenerator.Emit(OpCodes.Ldarg_2);
                ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                ilGenerator.Emit(OpCodes.Ldarg_2);
                ilGenerator.Emit(OpCodes.Box);
                ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                ilGenerator.Emit(OpCodes.Ldind_I4, 0);
                ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                ilGenerator.MarkLabel(loadArgumentLabel);
                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                ilGenerator.Emit(OpCodes.Ldind_I4, 0);
                ilGenerator.Emit(OpCodes.Ldarg_2);
                ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                ilGenerator.MarkLabel(loadNextArgumentLabel);

                if (wrappedTypeConstructorParameters.Length >= 2)
                {
                    loadArgumentLabel = ilGenerator.DefineLabel();
                    loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 1);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, 1);
                    ilGenerator.Emit(OpCodes.Ldarg_3);
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                for (short parameterIndex = 2; parameterIndex < wrappedTypeConstructorParameters.Length; parameterIndex++)
                {
                    loadArgumentLabel = ilGenerator.DefineLabel();
                    loadNextArgumentLabel = ilGenerator.DefineLabel();

                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 2));
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 2));
                    ilGenerator.Emit(OpCodes.Callvirt, objectGetTypeMethod);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
                    ilGenerator.Emit(OpCodes.Brfalse, loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 2));
                    ilGenerator.Emit(OpCodes.Box);
                    ilGenerator.Emit(OpCodes.Stloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldloc, localObjectParam);
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));
                    ilGenerator.Emit(OpCodes.Br, loadNextArgumentLabel);
                    ilGenerator.MarkLabel(loadArgumentLabel);
                    ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                    ilGenerator.Emit(OpCodes.Ldind_I4, (int)parameterIndex);
                    ilGenerator.Emit(OpCodes.Ldarg, (short)(parameterIndex + 2));
                    ilGenerator.Emit(OpCodes.Castclass, typeof(object));
                    ilGenerator.Emit(OpCodes.Stelem, typeof(object));

                    ilGenerator.MarkLabel(loadNextArgumentLabel);
                }

                ilGenerator.Emit(OpCodes.Ldloc, localOjectParamList);
                ilGenerator.Emit(OpCodes.Call, baseConstructor);
                ilGenerator.Emit(OpCodes.Nop);
                ilGenerator.Emit(OpCodes.Nop);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        private static Type ReplaceGenericArgumentsAndConstraintsFromType(Type type, Dictionary<Type, GenericTypeParameterBuilder> genericParametersMap)
        {
            if (!type.IsGenericTypeDefinition)
            {
                if (genericParametersMap.ContainsKey(type))
                {
                    foreach (var genericConstraint in type.GetGenericParameterConstraints())
                    {
                        var replacedGenericConstraint = ReplaceGenericArgumentsAndConstraintsFromType(genericConstraint, genericParametersMap);
                        if (genericParametersMap[type].GetGenericParameterConstraints().Any(constraint => replacedGenericConstraint == constraint))
                        {
                            continue;
                        }
                        if (replacedGenericConstraint.IsInterface)
                        {
                            genericParametersMap[type].SetInterfaceConstraints(replacedGenericConstraint);
                            continue;
                        }
                        genericParametersMap[type].SetBaseTypeConstraint(replacedGenericConstraint);
                    }
                    genericParametersMap[type].SetGenericParameterAttributes(type.GenericParameterAttributes & ~(GenericParameterAttributes.Covariant) & ~(GenericParameterAttributes.Contravariant));
                    return genericParametersMap[type];
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

        private static Type ReplaceGenericArgumentsFromType(Type type, Dictionary<Type, GenericTypeParameterBuilder>? genericParametersMap)
        {
            if (genericParametersMap == null)
            {
                return type;
            }

            if (genericParametersMap.ContainsKey(type))
            {
                return genericParametersMap[type];
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
