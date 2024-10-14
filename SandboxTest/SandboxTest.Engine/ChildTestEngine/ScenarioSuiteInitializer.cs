using SandboxTest.Instance;
using SandboxTest.Scenario;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class ScenarioSuiteInitializer : IScenarioSuiteInitializer
    {
        public void Initialize(object scenarioSuite)
        {
            var scenarioSuiteType = scenarioSuite.GetType();
            var scenarioSuiteNamePrefix = ScenarioSuiteTypeNamePrefix.SanitizeClassName(scenarioSuiteType.Name);
            var allScenarioSuitesFromAssembly = scenarioSuite.GetType().Assembly.GetTypes().Where(type => type.GetCustomAttribute<ScenarioSuiteAttribute>() != null);
            var allScenarioSuitesWithSameName = allScenarioSuitesFromAssembly.Where(otherScenarioSuiteType => otherScenarioSuiteType.Name.Equals(scenarioSuiteType.Name, StringComparison.InvariantCultureIgnoreCase) && otherScenarioSuiteType != scenarioSuiteType);

            if (allScenarioSuitesWithSameName.Any())
            {
                var scenarioSuiteNamePrefixes = new List<ScenarioSuiteTypeNamePrefix>();
                scenarioSuiteNamePrefixes.Add(new ScenarioSuiteTypeNamePrefix(scenarioSuiteType));
                scenarioSuiteNamePrefixes.AddRange(allScenarioSuitesWithSameName.Select(scenarioSuiteTypeSameName => new ScenarioSuiteTypeNamePrefix(scenarioSuiteTypeSameName)));

                while (scenarioSuiteNamePrefixes.Any(suiteNamePrefix => scenarioSuiteNamePrefixes.Any(suiteNamePrefix2 => suiteNamePrefix.Equals(suiteNamePrefix2))))
                {
                    foreach (var suiteNamePrefix in scenarioSuiteNamePrefixes)
                    {
                        if (suiteNamePrefix.CanAppendToPrefix())
                        {
                            suiteNamePrefix.AppendToPrefix();
                        }
                    }
                }

                scenarioSuiteNamePrefix = scenarioSuiteNamePrefixes.First().Prefix;
            }

            var instanceFields = GetInstancesMembers(scenarioSuite);

            foreach (var instanceField in instanceFields)
            {
                var instance = (IInstance)instanceField.GetValue(scenarioSuite)!;
                instance.Initialize($"{scenarioSuiteNamePrefix}.{instanceField.Name}");
            }
        }

        protected virtual IEnumerable<FieldInfo> GetInstancesMembers(object scenarioSuite)
        {
            if (scenarioSuite == null)
            {
                throw new InvalidOperationException("No scenario suite loaded in scenario suite test engine");
            }

            var instanceInterfaceType = typeof(IInstance);
            var instanceFields = new List<FieldInfo>();
            var allFields = new List<FieldInfo>();
            allFields.AddRange(scenarioSuite.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public));
            allFields.AddRange(scenarioSuite.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic));

            foreach (var field in allFields)
            {
                if (instanceInterfaceType.IsAssignableFrom(field.FieldType))
                {
                    instanceFields.Add(field);
                }
            }

            return instanceFields;
        }

        private class ScenarioSuiteTypeNamePrefix
        {
            public static string SanitizeClassName(string className)
            {
                return className.Replace('<', '_').Replace(">", "_");
            }

            public ScenarioSuiteTypeNamePrefix(Type type) 
            {
                Prefix = type.Name;
                NamespacePrefixes = new Stack<string>(type.Namespace!.Split('.'));
            }

            public string Prefix { get; set; }

            public Stack<string> NamespacePrefixes { get; set; }

            public bool Equals(ScenarioSuiteTypeNamePrefix other)
            {
                return other.Prefix.Equals(Prefix, StringComparison.InvariantCultureIgnoreCase);
            }

            public void AppendToPrefix()
            {
                Prefix = $"{NamespacePrefixes.Pop()}.{Prefix}";
            }

            public bool CanAppendToPrefix()
            {
                return NamespacePrefixes.Count > 0;
            }
        }
    }
}
