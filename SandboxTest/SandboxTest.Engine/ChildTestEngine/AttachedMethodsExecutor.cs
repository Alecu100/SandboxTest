using System.Reflection;
using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;

namespace SandboxTest.Engine.ChildTestEngine
{
    /// <inheritdoc/>
    public class AttachedMethodsExecutor : IAttachedMethodsExecutor
    {
        private static readonly IReadOnlyDictionary<AttachedMethodType, Type> AttachedMethodAllowedTargetTypes = new Dictionary<AttachedMethodType, Type>
        {
            {
                AttachedMethodType.RunnerToRunner,
                typeof(IRunner)
            },
                        {
                AttachedMethodType.ControllerToRunner,
                typeof(IRunner)
            }
        };

        /// <inheritdoc/>
        public async Task ExecutedMethods(IEnumerable<object> instances, IEnumerable<AttachedMethodType> includedStepTypes, Delegate targetMethod, IEnumerable<object> arguments)
        {
            var possibleMethodsToRun = new List<(AttachedMethodAttribute, MethodToExecute)>();

            foreach (var instance in instances) 
            { 
                var instanceType = instance.GetType();
                var instanceMethods = instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

                foreach (var method in instanceMethods) 
                {
                    var injectedMethodAttribute = GetAttachedMethodAttribute(instanceType, method);
                    if (injectedMethodAttribute != null && includedStepTypes.Contains(injectedMethodAttribute.MethodType))
                    {
                        possibleMethodsToRun.Add((injectedMethodAttribute, new MethodToExecute { Method = method, Target = instance }));
                    }
                }
            }

            await ExecuteMethodIncludingAttachedMethods(possibleMethodsToRun, new MethodToExecute { Target = targetMethod.Target!, Method = targetMethod.Method } , arguments);
        }

        private async Task ExecuteMethodIncludingAttachedMethods(List<(AttachedMethodAttribute, MethodToExecute)> possibleMethodsToRun, MethodToExecute targetMethod, IEnumerable<object> arguments)
        {
            var methodsToRunBeforeTargetMethod = GetMethodsToRunBeforeTargetMethod(possibleMethodsToRun, targetMethod);

            foreach (var methodToRunBeforeTargetMethod in methodsToRunBeforeTargetMethod)
            {
                await ExecuteMethodIncludingAttachedMethods(possibleMethodsToRun, methodToRunBeforeTargetMethod.Item2, arguments);
            }

            object? result;
            if (!targetMethod.Method.GetParameters().Any())
            {
                result = targetMethod.Method.Invoke(targetMethod.Target, null);
            }
            else
            {
                result = targetMethod.Method.Invoke(targetMethod.Target, arguments.ToArray());
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

        private List<(AttachedMethodAttribute, MethodToExecute)> GetMethodsToRunBeforeTargetMethod(List<(AttachedMethodAttribute, MethodToExecute)> possibleMethodsToRun, MethodToExecute targetMethod)
        {
            return possibleMethodsToRun
                .Where(methodWithAttribute => methodWithAttribute.Item1.TargetMethodName.Equals(targetMethod.Method.Name, StringComparison.InvariantCultureIgnoreCase) 
                    && methodWithAttribute.Item2.Method != targetMethod.Method && methodWithAttribute.Item1.Order < 0
                    && targetMethod.Target.GetType().IsAssignableTo(AttachedMethodAllowedTargetTypes[methodWithAttribute.Item1.MethodType]))
                .OrderBy(methodWithAttribute => methodWithAttribute.Item1.Order)
                .ToList();
        }

        private List<(AttachedMethodAttribute, MethodToExecute)> GetMethodsToRunAfterTargetMethod(List<(AttachedMethodAttribute, MethodToExecute)> possibleMethodsToRun, MethodToExecute targetMethod)
        {
            return possibleMethodsToRun
                .Where(methodWithAttribute => methodWithAttribute.Item1.TargetMethodName.Equals(targetMethod.Method.Name, StringComparison.InvariantCultureIgnoreCase) 
                && methodWithAttribute.Item2.Method != targetMethod.Method && methodWithAttribute.Item1.Order >= 0
                && targetMethod.Target.GetType().IsAssignableTo(AttachedMethodAllowedTargetTypes[methodWithAttribute.Item1.MethodType]))
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

        private class MethodToExecute
        {
            required public object Target { get; set; }

            required public MethodInfo Method { get; set; }
        }
    }
}
