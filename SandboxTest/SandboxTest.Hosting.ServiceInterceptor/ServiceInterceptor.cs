﻿using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.ServiceInterceptor
{
    public class ServiceInterceptor : DispatchProxy
    {
        protected object? _wrappedInstance;
        protected ServiceInterceptorController _controller;
        protected List<Type> _implementedInterfaceTypes;

        public ServiceInterceptor(ServiceInterceptorController controller, Type wrappedType, object?[]? arguments)
        {
            _controller = controller;
            _implementedInterfaceTypes = GetType().GetInterfaces().ToList();
            if (wrappedType.IsGenericTypeDefinition)
            {
                if (!GetType().IsGenericType)
                {
                    throw new InvalidOperationException("Non generic service interceptor contains generic wrapped type");
                }
                var actualType = wrappedType.MakeGenericType(GetType().GetGenericArguments());
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
            _implementedInterfaceTypes = GetType().GetInterfaces().ToList();
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
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap = null;
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap = null;
            List<MethodBuilder> builtMethods = new List<MethodBuilder>();

            if (interfaceType.IsGenericTypeDefinition)
            {
                if (!wrappedType.IsGenericTypeDefinition)
                {
                    throw new InvalidOperationException("Wrapped type is not an open generic type whereas the interface is");
                }
                var interfaceGenericArguments = interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                serviceInterceptorGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                wrappedTypeGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
                }
                var wrappedTypeGenericArguments = wrappedType.GetGenericArguments();
                for (int i = 0; i < wrappedTypeGenericArguments.Length; i++)
                {
                    wrappedTypeGenericParametersMap[wrappedTypeGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
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

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

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
            Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap = null;
            List<MethodBuilder> builtMethods = new List<MethodBuilder>();

            if (interfaceType.IsGenericTypeDefinition)
            {
                var interfaceGenericArguments = interfaceType.GetGenericArguments();
                serviceInterceptorGenericParameters = serviceInterceptorTypeBuilder.DefineGenericParameters(interfaceGenericArguments.Select(arg => $"{arg.Name}W").ToArray());
                serviceInterceptorGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                for (int i = 0; i < serviceInterceptorGenericParameters.Length; i++)
                {
                    serviceInterceptorGenericParametersMap[interfaceGenericArguments[i]] = new GeneraticParameterTypeWithInitialization { GenericTypeParameterBuilder = serviceInterceptorGenericParameters[i] };
                }

                interfaceType = ReplaceGenericArgumentsAndConstraintsFromType(interfaceType, serviceInterceptorGenericParametersMap);
            }

            serviceInterceptorTypeBuilder.AddInterfaceImplementation(interfaceType);

            GenerateConstructor(serviceInterceptorBaseType, serviceInterceptorTypeBuilder);

            GenerateInterfaceMethods(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceProperties(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            GenerateInterfaceEvents(interfaceType, serviceInterceptorTypeBuilder, serviceInterceptorGenericParametersMap, builtMethods);

            var createdType = serviceInterceptorTypeBuilder.CreateType();
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

        private static void GenerateInterfaceEvents(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var interfaceEvents = interfaceType.GetEvents(BindingFlags.Instance);

            foreach (var interfaceEvent in interfaceEvents)
            {
                if (interfaceEvent.EventHandlerType == null)
                {
                    continue;
                }
                var interfaceEventImplementationDelegateType = ReplaceGenericArgumentsFromType(interfaceEvent.EventHandlerType, serviceInterceptorGenericParametersMap);
                var interfaceEventImplementation = serviceInterceptorTypeBuilder.DefineEvent(interfaceEvent.Name, interfaceEvent.Attributes, interfaceEventImplementationDelegateType);
                var removeEventMethod = builtMethods.FirstOrDefault(method => method.Name == $"remove_{interfaceEvent.Name}");
                var addEventMethod = builtMethods.FirstOrDefault(method => method.Name == $"add_{interfaceEvent.Name}");
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

        private static void GenerateInterfaceProperties(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var interfaceProperties = GetAllInterfaceProperties(interfaceType);

            foreach (var interfaceProperty in interfaceProperties)
            {
                var returnType = ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, serviceInterceptorGenericParametersMap);
                var parameterTypes = interfaceProperty.GetIndexParameters().Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, serviceInterceptorGenericParametersMap)).ToArray();
                var property = serviceInterceptorTypeBuilder.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, serviceInterceptorGenericParametersMap), parameterTypes);
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

        private static void GenerateInterfaceMethods(Type interfaceType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GeneraticParameterTypeWithInitialization>? serviceInterceptorGenericParametersMap, List<MethodBuilder> builtMethods)
        {
            var getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
            var invokeMethod = typeof(ServiceInterceptor).GetMethod(nameof(Invoke), BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get current method");
            var getCurrentMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Could not get current method");
            var interfaceMethods = GetAllInterfaceMethods(interfaceType);
            var objectGetTypeMethod = typeof(ServiceInterceptor).GetType().GetMethod(nameof(GetType), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not get get type object method");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");

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
                                var otherInterfaceMethodGenericArguments = otherInterfaceMethod.DeclaringType.GetGenericArguments().Select(arg => ReplaceGenericArgumentsFromType(arg, serviceInterceptorGenericParametersMap)).ToArray();
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
                var interfaceMethodImplementation = serviceInterceptorTypeBuilder.DefineMethod(interfaceMethodImplementationName, interfaceMethod.Attributes & ~MethodAttributes.Abstract, interfaceMethod.CallingConvention);
                builtMethods.Add(interfaceMethodImplementation);
                var interfaceMethodArgumentsType = new List<Type>();
                var interfaceMethodGenericArguments = interfaceMethod.GetGenericArguments();
                var interfaceMethodGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                if (serviceInterceptorGenericParametersMap != null && serviceInterceptorGenericParametersMap.Any())
                {
                    foreach (var parameterMap in serviceInterceptorGenericParametersMap)
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

                var interfaceMethodImplementationParameters = interfaceMethod.GetParameters().Select(x => ReplaceGenericArgumentsFromType(x.ParameterType, serviceInterceptorGenericParametersMap)).ToArray();
                var interfaceMethodReturnType = ReplaceGenericArgumentsFromType(interfaceMethod.ReturnType ?? typeof(void), serviceInterceptorGenericParametersMap);
                interfaceMethodImplementation.SetParameters(interfaceMethodImplementationParameters);
                interfaceMethodImplementation.SetReturnType(interfaceMethodReturnType);

                if (isExplicitMethodImplementation)
                {
                    serviceInterceptorTypeBuilder.DefineMethodOverride(interfaceMethodImplementation, interfaceMethodToExplicitlyImplement!);
                }

                var ilGenerator = interfaceMethodImplementation.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                var localMethodInfo = ilGenerator.DeclareLocal(typeof(MethodInfo));
                var localInvokeReturnedObject = ilGenerator.DeclareLocal(typeof(object));
                var localUnboxedInvokeReturnedObject = ilGenerator.DeclareLocal(interfaceMethodReturnType.GetType());
                ilGenerator.EmitWriteLine("Calling generated interface method");
                ilGenerator.Emit(OpCodes.Call, getCurrentMethod);
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
                    ilGenerator.Emit(OpCodes.Call, getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
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
                ilGenerator.Emit(OpCodes.Callvirt, invokeMethod);
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
                    ilGenerator.Emit(OpCodes.Call, getTypeFromHandle);
                    ilGenerator.Emit(OpCodes.Callvirt, objectTypeIsValueTypeGetMethod);
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
                    ilGenerator.Emit(OpCodes.Call, getTypeFromHandle);
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

        private static void GenerateConstructors(Type wrappedType, Type serviceInterceptorBaseType, TypeBuilder serviceInterceptorTypeBuilder, Dictionary<Type, GeneraticParameterTypeWithInitialization>? wrappedTypeGenericParametersMap, ServiceInterceptorController controller)
        {
            var getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
            var objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");
            var serviceInterceptorControllerType = typeof(ServiceInterceptorController);
            var baseConstructor = serviceInterceptorBaseType.GetConstructor(new[] { typeof(ServiceInterceptorController), typeof(Type), typeof(object?[]) }) ?? throw new InvalidOperationException("Could not find proper base constructor of service interceptor type");
            var wrappedTypeConstructors = wrappedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var gcHandleWrappedType = GCHandle.Alloc(wrappedType);
            var ptrHandleWrappedType = GCHandle.ToIntPtr(gcHandleWrappedType);

            controller.AddReference(wrappedType);

            foreach (var wrappedTypeConstructor in wrappedTypeConstructors)
            {
                var wrappedTypeConstructorParameters = wrappedTypeConstructor.GetParameters().Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, wrappedTypeGenericParametersMap)).ToArray();
                var constructorParameters = new[] { serviceInterceptorControllerType }.Concat(wrappedTypeConstructorParameters).ToArray();
                var constructor = serviceInterceptorTypeBuilder.DefineConstructor(wrappedTypeConstructor.Attributes, wrappedTypeConstructor.CallingConvention, constructorParameters);
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

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (_implementedInterfaceTypes == null || _controller == null || _wrappedInstance == null)
            {
                throw new InvalidOperationException("Proxy interceptor not initialized");
            }
            if (targetMethod == null)
            {
                throw new InvalidOperationException("Null target method");
            }
            var currentType = GetType();
            if (!targetMethod.IsGenericMethodDefinition && currentType.IsGenericType)
            {
                targetMethod = MethodBase.GetMethodFromHandle(targetMethod.MethodHandle, currentType.TypeHandle) as MethodInfo;
            }
            if (targetMethod == null)
            {
                throw new InvalidOperationException("Null target method");
            }
            foreach (var @interface in _implementedInterfaceTypes)
            {
                var interfaceMap = currentType.GetInterfaceMap(@interface);
                for (int i = 0; i < interfaceMap.TargetMethods.Length; i++) 
                {
                    var interfaceImplementedMethod = interfaceMap.TargetMethods[i];
                    if (interfaceImplementedMethod == targetMethod)
                    {
                        targetMethod = interfaceMap.InterfaceMethods[i];
                        break;
                    }
                }
            }

            foreach (var interfaceType in _implementedInterfaceTypes)
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

        private class GeneraticParameterTypeWithInitialization
        {
            required public GenericTypeParameterBuilder GenericTypeParameterBuilder { get; set; }

            public bool IsInitialized { get; set; } = false;
        }
    }
}
