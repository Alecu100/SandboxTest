using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class MainTestEngine : IMainTestEngine
    {
        public virtual void OnScenarionRan(Func<ScenarioRunResult, Task> onScenarioRanAsyncCallback)
        {
            throw new NotImplementedException();
        }

        public virtual Task RunScenariosAsync(string assemblyPath, IEnumerable<ScenarioParameters> scenarioRunParameters)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<ScenarioParameters> ScanForScenarios(string assemblyPath)
        {
            var foundScenarioParameters = new List<ScenarioParameters>();
            var scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(assemblyPath);
            var scenariosAssembly = scenariosAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath);

            foreach (var assemblyType in scenariosAssembly.GetTypes())
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
            scenariosAssemblyLoadContext.Unload();

            return foundScenarioParameters;
        }
    }
}
