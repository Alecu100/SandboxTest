using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SandboxTest.Hosting.ServiceInterceptor.Internal
{
    public abstract class ServiceInterceptorTypeBuilderBase
    {
        protected static MethodInfo _getTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) }) ?? throw new InvalidOperationException("Could not get method get type from handle");
        protected static MethodInfo _invokeMethod = typeof(ServiceInterceptor).GetMethod(ServiceInterceptor.InvokeMethodName, BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Could not get current method");
        protected static MethodInfo _getCurrentMethod = typeof(MethodBase).GetMethod(nameof(MethodBase.GetCurrentMethod), BindingFlags.Static | BindingFlags.Public) ?? throw new InvalidOperationException("Could not get current method");
        protected static MethodInfo _objectGetTypeMethod = typeof(ServiceInterceptor).GetType().GetMethod(nameof(GetType), BindingFlags.Public | BindingFlags.Instance) ?? throw new InvalidOperationException("Could not get get type object method");
        protected static MethodInfo _objectTypeIsValueTypeGetMethod = typeof(ServiceInterceptor).GetType().GetProperty(nameof(Type.IsValueType), BindingFlags.Public | BindingFlags.Instance)?.GetMethod ?? throw new InvalidOperationException("Could not get the method is value type");
        protected static Type _serviceInterceptorBaseType = typeof(ServiceInterceptor);
        protected static Type _voidType = typeof(void);

        protected readonly Type _interfaceType;

        protected TypeBuilder? _serviceInterceptorTypeBuilder;
        protected Dictionary<Type, GeneraticParameterTypeWithInitialization>? _serviceInterceptorGenericParametersMap = null;
        protected Type? _implementedInterfaceType;
        protected List<Type>? _allImplementedInterfaces;
        protected Dictionary<MethodInfo, MethodBuilder>? _builtInterfaceMethods;
        protected GCHandle _serviceInterceptorHandle;
        protected ServiceInterceptorAssembly _serviceInterceptorAssembly;

        protected ServiceInterceptorTypeBuilderBase(Type interfaceType, GCHandle serviceInterceptorHandle, ServiceInterceptorAssembly serviceInterceptorAssembly)
        {
            _interfaceType = interfaceType;
            _serviceInterceptorAssembly = serviceInterceptorAssembly;
            _serviceInterceptorHandle = serviceInterceptorHandle;
        }


        public abstract Type Build();
        

        protected List<Type> GetAllImplementedInterfaces(Type implementedInterfaceType)
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

        protected static bool InterfacesAreEquivalent(Type interface1, Type interface2)
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

        protected void GenerateInterfaceEvents()
        {
            var interfaceEvents = GetAllInterfaceEvents(_interfaceType);

            foreach (var interfaceEvent in interfaceEvents)
            {
                var isExplicitEventImplementation = false;

                if (interfaceEvent.EventHandlerType == null)
                {
                    continue;
                }
                foreach (var otherInterfaceEvent in interfaceEvents)
                {
                    if (otherInterfaceEvent == interfaceEvent || otherInterfaceEvent.Name != interfaceEvent.Name)
                    {
                        continue;
                    }

                    if (!interfaceEvent!.DeclaringType!.IsAssignableTo(otherInterfaceEvent.DeclaringType))
                    {
                        isExplicitEventImplementation = true;
                        break;
                    }
                }
                var implementedInterfaceEventName = isExplicitEventImplementation ? $"{interfaceEvent.DeclaringType}.{interfaceEvent.Name}" : interfaceEvent.Name;
                var interfaceEventImplementationDelegateType = ReplaceGenericArgumentsFromType(interfaceEvent.EventHandlerType, _serviceInterceptorGenericParametersMap);
                var interfaceEventImplementation = _serviceInterceptorTypeBuilder!.DefineEvent(implementedInterfaceEventName, interfaceEvent.Attributes, interfaceEventImplementationDelegateType);
                if (interfaceEvent.RemoveMethod != null && _builtInterfaceMethods!.ContainsKey(interfaceEvent.RemoveMethod))
                {
                    interfaceEventImplementation.SetRemoveOnMethod(_builtInterfaceMethods[interfaceEvent.RemoveMethod]);
                }
                if (interfaceEvent.AddMethod != null && _builtInterfaceMethods!.ContainsKey(interfaceEvent.AddMethod))
                {
                    interfaceEventImplementation.SetAddOnMethod(_builtInterfaceMethods[interfaceEvent.AddMethod]);
                }
            }
        }

        protected void GenerateInterfaceProperties()
        {
            var interfaceProperties = GetAllInterfaceProperties(_interfaceType);

            foreach (var interfaceProperty in interfaceProperties)
            {
                var isExplicitPropertyImplementation = false;

                foreach (var otherInterfacePropery in interfaceProperties)
                {
                    if (otherInterfacePropery == interfaceProperty || otherInterfacePropery.Name != interfaceProperty.Name)
                    {
                        continue;
                    }

                    if (!interfaceProperty!.DeclaringType!.IsAssignableTo(otherInterfacePropery.DeclaringType))
                    {
                        isExplicitPropertyImplementation = true;
                        break;
                    }
                }
                var implementedInterfacePropertyName = isExplicitPropertyImplementation ? $"{interfaceProperty.DeclaringType}.{interfaceProperty.Name}" : interfaceProperty.Name;
                var implementedInterfacePropertyReturnType = ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, _serviceInterceptorGenericParametersMap);
                var implementedInterfacePropertyParameterTypes = interfaceProperty.GetIndexParameters().Select(param => ReplaceGenericArgumentsFromType(param.ParameterType, _serviceInterceptorGenericParametersMap)).ToArray();
                var implementedInterfaceProperty = _serviceInterceptorTypeBuilder!.DefineProperty(interfaceProperty.Name, interfaceProperty.Attributes, ReplaceGenericArgumentsFromType(interfaceProperty.PropertyType, _serviceInterceptorGenericParametersMap), implementedInterfacePropertyParameterTypes);
                if (interfaceProperty.GetMethod != null && _builtInterfaceMethods!.ContainsKey(interfaceProperty.GetMethod))
                {
                    implementedInterfaceProperty.SetGetMethod(_builtInterfaceMethods![interfaceProperty.GetMethod]);
                }
                if (interfaceProperty.SetMethod != null && _builtInterfaceMethods!.ContainsKey(interfaceProperty.SetMethod))
                {
                    implementedInterfaceProperty.SetGetMethod(_builtInterfaceMethods![interfaceProperty.SetMethod]);
                }
            }

        }

        protected void GenerateInterfaceMethods()
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
                        if (otherInterfaceMethodParameters[parameterIndex].ParameterType != interfaceMethodParameters[parameterIndex].ParameterType
                            && !otherInterfaceMethodParameters[parameterIndex].ParameterType.IsGenericMethodParameter
                            && !interfaceMethodParameters[parameterIndex].ParameterType.IsGenericMethodParameter)
                        {
                            continue;
                        }
                    }
                    if (!interfaceMethod.DeclaringType.IsAssignableTo(otherInterfaceMethod.DeclaringType))
                    {
                        isExplicitMethodImplementation = true;
                        interfaceMethodToExplicitlyImplement = interfaceMethod;
                        break;
                    }
                }
                if (isExplicitMethodImplementation && (interfaceMethodToExplicitlyImplement == null || interfaceMethodToExplicitlyImplement.DeclaringType == null))
                {
                    throw new InvalidOperationException("Could not find method to explicitly implement");
                }
                var interfaceMethodImplementationAttributes = interfaceMethod.Attributes & ~MethodAttributes.Abstract | MethodAttributes.Virtual;
                if (isExplicitMethodImplementation)
                {
                    interfaceMethodImplementationAttributes &= ~MethodAttributes.Public;
                    interfaceMethodImplementationAttributes |= MethodAttributes.Private;
                    interfaceMethodImplementationAttributes |= MethodAttributes.Final;
                }
                var interfaceMethodImplementationName = isExplicitMethodImplementation ? $"{interfaceMethodToExplicitlyImplement!.DeclaringType!.Name}.{interfaceMethod.Name}" : interfaceMethod.Name;
                var interfaceMethodImplementation = _serviceInterceptorTypeBuilder!.DefineMethod(interfaceMethodImplementationName, interfaceMethodImplementationAttributes, interfaceMethod.CallingConvention);
                _builtInterfaceMethods!.Add(interfaceMethod, interfaceMethodImplementation);
                var interfaceMethodArgumentsType = new List<Type>();
                var interfaceMethodGenericArguments = interfaceMethod.GetGenericArguments();
                var interfaceMethodGenericParametersMap = new Dictionary<Type, GeneraticParameterTypeWithInitialization>();
                GenericTypeParameterBuilder[]? genericMethodParameters = null;
                if (_serviceInterceptorGenericParametersMap != null && _serviceInterceptorGenericParametersMap.Any())
                {
                    foreach (var parameterMap in _serviceInterceptorGenericParametersMap)
                    {
                        interfaceMethodGenericParametersMap[parameterMap.Key] = parameterMap.Value;
                    }
                }
                if (interfaceMethodGenericArguments.Any())
                {
                    genericMethodParameters = interfaceMethodImplementation.DefineGenericParameters(interfaceMethodGenericArguments.Select(arg => arg.Name).ToArray());
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
                                interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder!.SetInterfaceConstraints(replacedConstraint);
                                continue;
                            }
                            interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder!.SetBaseTypeConstraint(replacedConstraint);
                        }
                        interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].GenericTypeParameterBuilder!.SetGenericParameterAttributes(interfaceMethodGenericArgument.GenericParameterAttributes & ~GenericParameterAttributes.Covariant & ~GenericParameterAttributes.Contravariant);
                        interfaceMethodGenericParametersMap[interfaceMethodGenericArgument].IsInitialized = true;
                    }
                }

                var interfaceMethodImplementationParameters = interfaceMethod.GetParameters().Select(x => ReplaceGenericArgumentsFromType(x.ParameterType, interfaceMethodGenericParametersMap)).ToArray();
                var interfaceMethodReturnType = ReplaceGenericArgumentsFromType(interfaceMethod.ReturnType ?? typeof(void), interfaceMethodGenericParametersMap);
                interfaceMethodImplementation.SetParameters(interfaceMethodImplementationParameters);
                interfaceMethodImplementation.SetReturnType(interfaceMethodReturnType);

                _serviceInterceptorTypeBuilder.DefineMethodOverride(interfaceMethodImplementation, interfaceMethod!);

                var ilGenerator = interfaceMethodImplementation.GetILGenerator();
                var localOjectParamList = ilGenerator.DeclareLocal(typeof(object[]));
                var localOjectGenericParamTypeList = ilGenerator.DeclareLocal(typeof(Type[]));
                var localObjectParam = ilGenerator.DeclareLocal(typeof(object));
                var localMethodInfo = ilGenerator.DeclareLocal(typeof(MethodInfo));
                var localInvokeReturnedObject = ilGenerator.DeclareLocal(typeof(object));
                var localUnboxedInvokeReturnedObject = ilGenerator.DeclareLocal(interfaceMethodReturnType == _voidType ? typeof(object) : interfaceMethodReturnType);
                ilGenerator.EmitWriteLine("Calling generated interface method");
                ilGenerator.Emit(OpCodes.Call, _getCurrentMethod);
                ilGenerator.Emit(OpCodes.Castclass, typeof(MethodInfo));
                ilGenerator.Emit(OpCodes.Stloc, localMethodInfo);

                ilGenerator.Emit(OpCodes.Ldc_I4, interfaceMethodImplementationParameters.Length);
                ilGenerator.Emit(OpCodes.Newarr, typeof(object));
                ilGenerator.Emit(OpCodes.Stloc, localOjectParamList);

                if (genericMethodParameters != null && genericMethodParameters.Any())
                {
                    ilGenerator.Emit(OpCodes.Ldc_I4, genericMethodParameters.Length);
                    ilGenerator.Emit(OpCodes.Newarr, typeof(Type));
                    ilGenerator.Emit(OpCodes.Stloc, localOjectGenericParamTypeList);

                    for (short genericParameterIndex = 0; genericParameterIndex < genericMethodParameters.Length; genericParameterIndex++)
                    {
                        ilGenerator.Emit(OpCodes.Ldloc, localOjectGenericParamTypeList);
                        ilGenerator.Emit(OpCodes.Ldc_I4, (int)genericParameterIndex);
                        ilGenerator.Emit(OpCodes.Ldtoken, genericMethodParameters[genericParameterIndex]);
                        ilGenerator.Emit(OpCodes.Call, _getTypeFromHandle);
                        ilGenerator.Emit(OpCodes.Stelem_Ref);
                    }
                }
                else
                {
                    ilGenerator.Emit(OpCodes.Ldnull);
                    ilGenerator.Emit(OpCodes.Stloc, localOjectGenericParamTypeList);
                }

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
                ilGenerator.Emit(OpCodes.Ldloc, localOjectGenericParamTypeList);
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
                    ilGenerator.Emit(OpCodes.Unbox_Any, interfaceMethodReturnType);
                    ilGenerator.Emit(OpCodes.Br, returnLabel);
                }

                ilGenerator.MarkLabel(returnLabel);
                ilGenerator.Emit(OpCodes.Ret);
            }
        }

        protected static string MakeSafeName(string name)
        {
            return name.Replace("'", "").Replace("`", "").Replace("-", "_").Replace(",", "").Replace(";", "");
        }

        protected static List<Type> GetAllInterfacesImplementedByType(Type type)
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

        protected static List<MethodInfo> GetAllInterfaceMethods(Type interfaceType, HashSet<Type>? scannedInterfaces = null)
        {
            if (scannedInterfaces == null)
            {
                scannedInterfaces = new HashSet<Type>();
            }
            if (scannedInterfaces.Contains(interfaceType))
            {
                return new List<MethodInfo>();
            }
            scannedInterfaces.Add(interfaceType);
            var interfaceMethods = interfaceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList();
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceMethods.AddRange(GetAllInterfaceMethods(implementedInterface, scannedInterfaces));
            }

            return interfaceMethods;
        }

        private static List<PropertyInfo> GetAllInterfaceProperties(Type interfaceType, HashSet<Type>? scannedInterfaces = null)
        {
            if (scannedInterfaces == null)
            {
                scannedInterfaces = new HashSet<Type>();
            }
            if (scannedInterfaces.Contains(interfaceType))
            {
                return new List<PropertyInfo>();
            }
            scannedInterfaces.Add(interfaceType);
            var interfaceProperties = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            interfaceProperties.AddRange(interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic));
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceProperties.AddRange(GetAllInterfaceProperties(implementedInterface, scannedInterfaces));
            }

            return interfaceProperties;
        }

        protected static List<EventInfo> GetAllInterfaceEvents(Type interfaceType, HashSet<Type>? scannedInterfaces = null)
        {
            if (scannedInterfaces == null)
            {
                scannedInterfaces = new HashSet<Type>();
            }
            if (scannedInterfaces.Contains(interfaceType))
            {
                return new List<EventInfo>();
            }
            var interfaceEvents = interfaceType.GetEvents(BindingFlags.Instance | BindingFlags.Public).ToList();
            interfaceEvents.AddRange(interfaceType.GetEvents(BindingFlags.Instance | BindingFlags.NonPublic));
            var implementedInterfaces = interfaceType.GetInterfaces();
            foreach (var implementedInterface in implementedInterfaces)
            {
                interfaceEvents.AddRange(GetAllInterfaceEvents(implementedInterface, scannedInterfaces));
            }

            return interfaceEvents;
        }

        protected static Type ReplaceGenericArgumentsAndConstraintsFromType(Type type, Dictionary<Type, GeneraticParameterTypeWithInitialization> genericParametersMap)
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
                                genericParametersMap[type].GenericTypeParameterBuilder!.SetInterfaceConstraints(replacedGenericConstraint);
                                continue;
                            }
                            genericParametersMap[type].GenericTypeParameterBuilder!.SetBaseTypeConstraint(replacedGenericConstraint);
                        }
                        genericParametersMap[type].GenericTypeParameterBuilder!.SetGenericParameterAttributes(type.GenericParameterAttributes & ~GenericParameterAttributes.Covariant & ~GenericParameterAttributes.Contravariant);
                    }

                    return genericParametersMap[type].GenericTypeParameterBuilder!;
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

        protected static Type ReplaceGenericArgumentsFromType(Type type, Dictionary<Type, GeneraticParameterTypeWithInitialization>? genericParametersMap)
        {
            if (genericParametersMap == null)
            {
                return type;
            }

            if (genericParametersMap.ContainsKey(type))
            {
                return genericParametersMap[type].GenericTypeParameterBuilder!;
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

        protected class GeneraticParameterTypeWithInitialization
        {
            public GenericTypeParameterBuilder? GenericTypeParameterBuilder { get; set; }

            public bool IsInitialized { get; set; } = false;
        }
    }
}
