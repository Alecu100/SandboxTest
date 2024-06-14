using System.Reflection;
using System.Runtime.CompilerServices;

namespace SandboxTest.Engine.ChildTestEngine
{
    /// <inheritdoc/>
    public class AttachedMethodsExecutor : IAttachedMethodsExecutor
    {
        /// <inheritdoc/>
        public async Task ExecutedMethods(IEnumerable<object> instances, IEnumerable<AttachedMethodType> includedStepTypes, Delegate targetMethod, List<object> arguments)
        {
            var possibleMethodsToRun = new List<(AttachedMethodAttribute, Delegate)>();

            foreach (var instance in instances) 
            { 
                var instanceType = instance.GetType();
                var instanceMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in instanceMethods) 
                {
                    var injectedMethodAttribute = GetAttachedMethodAttribute(instanceType, method);
                    if (injectedMethodAttribute != null && includedStepTypes.Contains(injectedMethodAttribute.InjectedStepType))
                    {
                        possibleMethodsToRun.Add((injectedMethodAttribute, Delegate.CreateDelegate(instanceType, instance, method, true)!));
                    }
                }
            }

            await ExecuteMethodIncludingAttachedMethods(possibleMethodsToRun, targetMethod, arguments);
        }

        private async Task ExecuteMethodIncludingAttachedMethods(List<(AttachedMethodAttribute, Delegate)> possibleMethodsToRun, Delegate targetMethod, List<object> arguments)
        {
            var methodsToRunBeforeTargetMethod = GetMethodsToRunBeforeTargetMethod(possibleMethodsToRun, targetMethod);

            foreach (var methodToRunBeforeTargetMethod in methodsToRunBeforeTargetMethod)
            {
                await ExecuteMethodIncludingAttachedMethods(possibleMethodsToRun, methodToRunBeforeTargetMethod.Item2, arguments);
            }

            object? result = null;
            if (!targetMethod.Method.GetParameters().Any())
            {
                result = targetMethod.DynamicInvoke();
            }
            else
            {
                result = targetMethod.DynamicInvoke(arguments);
            }

            var resultTask = result as Task;
            if (resultTask != null)
            {
                await resultTask;
            }

            var methodsToRunAfterTargetMethod = GetMethodsToRunAfterTargetMethod(possibleMethodsToRun, targetMethod);

            foreach (var methodToRunAfterTargetMethod in methodsToRunAfterTargetMethod)
            {
                await ExecuteMethodIncludingAttachedMethods(possibleMethodsToRun, methodToRunAfterTargetMethod.Item2, arguments);
            }
        }

        private List<(AttachedMethodAttribute, Delegate)> GetMethodsToRunBeforeTargetMethod(List<(AttachedMethodAttribute, Delegate)> possibleMethodsToRun, Delegate targetMethod)
        {
            return possibleMethodsToRun
                .Where(methodWithAttribute => methodWithAttribute.Item1.TargetMethodName.Equals(targetMethod.Method.Name, StringComparison.InvariantCultureIgnoreCase) && methodWithAttribute.Item2.Method != targetMethod.Method && methodWithAttribute.Item1.Order < 0)
                .OrderBy(methodWithAttribute => methodWithAttribute.Item1.Order)
                .ToList();
        }

        private List<(AttachedMethodAttribute, Delegate)> GetMethodsToRunAfterTargetMethod(List<(AttachedMethodAttribute, Delegate)> possibleMethodsToRun, Delegate targetMethod)
        {
            return possibleMethodsToRun
                .Where(methodWithAttribute => methodWithAttribute.Item1.TargetMethodName.Equals(targetMethod.Method.Name, StringComparison.InvariantCultureIgnoreCase) && methodWithAttribute.Item2.Method != targetMethod.Method && methodWithAttribute.Item1.Order >= 0)
                .OrderBy(methodWithAttribute => methodWithAttribute.Item1.Order)
                .ToList();
        }

        private AttachedMethodAttribute? GetAttachedMethodAttribute(Type type, MethodInfo methodInfo)
        {
            var injectedMethodAttributes = new List<AttachedMethodAttribute>();
            injectedMethodAttributes.AddRange(methodInfo.GetCustomAttributes<AttachedMethodAttribute>(true));
            var allInterfaces = type.GetInterfaces();

            foreach(var @interface in allInterfaces)
            {
                var interfaceMethod = GetInterfaceMethod(type, @interface, methodInfo);

                if (interfaceMethod != null)
                {
                    injectedMethodAttributes.AddRange(interfaceMethod.GetCustomAttributes<AttachedMethodAttribute>());
                    break;
                }
            }

            return injectedMethodAttributes.FirstOrDefault();
        }

        private static MethodInfo? GetInterfaceMethod(Type implementingClass, Type implementedInterface, MethodInfo classMethod)
        {
            var map = implementingClass.GetInterfaceMap(implementedInterface);
            var index = Array.IndexOf(map.TargetMethods, classMethod);
            return index != -1 ?  map.InterfaceMethods[index] : null;
        }
    }
}
