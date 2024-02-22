using SandboxTest.Engine.Operations;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class ChildTestEngine : IChildTestEngine
    {
        private Type? _scenarionSuit;
        private ScenariosAssemblyLoadContext? _scenariosAssemblyLoadContext;
        private Assembly? _scenariosAssembly;

        public virtual async Task LoadSuiteAsync(string sourceAssembly, string scenarionSuiteName)
        {
            var foundScenarioParameters = new List<ScenarioParameters>();
            _scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(sourceAssembly);
            _scenariosAssembly = _scenariosAssemblyLoadContext.LoadFromAssemblyPath(sourceAssembly);

            foreach (var assemblyType in _scenariosAssembly.GetTypes())
            {
                if (assemblyType.FullName == null)
                {
                    continue;
                }
                var scenarioSuiteAttribute = assemblyType.CustomAttributes.FirstOrDefault(attribute => attribute.AttributeType.AssemblyQualifiedName == typeof(ScenarioSuiteAttribute).AssemblyQualifiedName);
                if (scenarioSuiteAttribute == null)
                {
                    continue;
                }
                string? scenarioSuiteName = null;
                if (scenarioSuiteAttribute.NamedArguments.Any(arg => arg.MemberName == nameof(ScenarioSuiteAttribute.Name)))
                {
                    scenarioSuiteName = (string?)scenarioSuiteAttribute.NamedArguments.FirstOrDefault(arg => arg.MemberName == nameof(ScenarioSuiteAttribute.Name)).TypedValue.Value;
                }
                var methodInfos = assemblyType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var methodInfo in methodInfos)
                {
                    var scenarioAttribute = methodInfo.CustomAttributes.FirstOrDefault(attribute => attribute.AttributeType.AssemblyQualifiedName == typeof(ScenarioAttribute).AssemblyQualifiedName);
                    if (scenarioAttribute == null)
                    {
                        continue;
                    }
                    string? scenarioDescription = null;
                    if (scenarioAttribute.NamedArguments.Any(arg => arg.MemberName == nameof(ScenarioAttribute.Description)))
                    {
                        scenarioSuiteName = (string?)scenarioSuiteAttribute.NamedArguments.FirstOrDefault(arg => arg.MemberName == nameof(ScenarioAttribute.Description)).TypedValue.Value;
                    }
                    var scenarioParameters = new ScenarioParameters(assemblyType.FullName, methodInfo.Name, scenarioDescription, scenarioSuiteName);
                    foundScenarioParameters.Add(scenarioParameters);
                }
            }

        }

        public virtual Task<OperationResult> BuildScenarioAsync(string scenarioMethodName)
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> ResetApplicationInstanceAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> RunApplicationInstanceAsync(string assemblyPath, string scenarioContainerFullyQualifiedName, string applicationInstanceId)
        {
            throw new NotImplementedException();
        }

        public virtual Task<OperationResult> RunStep(int stepIndex)
        {
            throw new NotImplementedException();
        }
    }
}
