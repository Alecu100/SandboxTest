using System.Diagnostics;
using System.Reflection;
using SandboxTest.Loader;
using SandboxTest.Scenario;

namespace SandboxTest.Engine.MainTestEngine
{
    public class MainTestEngine : IMainTestEngine
    {
        protected List<IScenarioSuiteTestEngine> _runningScenarioSuiteTestEngines;
        protected CancellationTokenSource? _cancellationTokenSource;

        public MainTestEngine()
        {
            _runningScenarioSuiteTestEngines = new List<IScenarioSuiteTestEngine>();
        }
        public virtual async Task RunScenariosAsync(IEnumerable<Scenario> scenarios, IMainTestEngineRunContext runContext)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _runningScenarioSuiteTestEngines.Clear();

            var scenarioSuitesAssemblies = scenarios.GroupBy(scenario => new
            {
                scenario.ScenarioSourceAssembly,
            });

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            var scenariosRunningTasks = new List<Task>();
            var assemblyLoadContexts = new List<ScenariosAssemblyLoadContext>();
            foreach (var scenarioSuitesAssembly in scenarioSuitesAssemblies)
            {
                var scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(scenarioSuitesAssembly.Key.ScenarioSourceAssembly);
                using var reflectionScope = scenariosAssemblyLoadContext.EnterContextualReflection();
                var scenariosAssembly = scenariosAssemblyLoadContext.LoadFromAssemblyPath(scenarioSuitesAssembly.Key.ScenarioSourceAssembly);
                assemblyLoadContexts.Add(scenariosAssemblyLoadContext);
                var scenarioSuites = scenarioSuitesAssembly.GroupBy(x => new { x.ScenarioSuitTypeFullName });
                foreach (var scenarioSuite in scenarioSuites)
                {
                    scenariosRunningTasks.Add(RunScenarioSuite(scenariosAssembly, scenariosAssemblyLoadContext, scenarioSuite.Key.ScenarioSuitTypeFullName, scenarioSuite, runContext));
                }
                
                await Task.WhenAll(scenariosRunningTasks);

                foreach (var assemblyLoadContext in assemblyLoadContexts)
                {
                    assemblyLoadContext.Unload();
                }
            }
        }

        protected virtual async Task RunScenarioSuite(Assembly scenariosAssembly, ScenariosAssemblyLoadContext scenariosAssemblyLoadContext, string scenarioSourceFullTypeName, IEnumerable<Scenario> scenarios, IMainTestEngineRunContext runContext)
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested || !scenarios.Any())
            {
                return;
            }

            var assemblyScenarioSuiteType = scenariosAssembly.GetType(scenarioSourceFullTypeName, false);
            if (assemblyScenarioSuiteType == null) 
            {
                foreach (var scenario in scenarios)
                {
                    var scenarioNotFoundResult = new ScenarioRunResult(ScenarioRunResultType.NotFound, scenario, DateTimeOffset.UtcNow, TimeSpan.Zero, $"Could not find scenario suite type {scenarioSourceFullTypeName}");
                    await runContext.OnScenarioRanAsync(scenarioNotFoundResult);
                }
                return;
            }

            var scenarioMethods = new List<MethodInfo>();
            foreach (var scenario in scenarios)
            {
                var methodInfo = assemblyScenarioSuiteType.GetMethod(scenario.ScenarioMethodName, BindingFlags.Instance | BindingFlags.Public);
                if (methodInfo == null)
                {
                    var scenarioNotFoundResult = new ScenarioRunResult(ScenarioRunResultType.NotFound, scenario, DateTimeOffset.UtcNow, TimeSpan.Zero, $"Could not find scenario method {scenario.ScenarioMethodName}");
                    await runContext.OnScenarioRanAsync(scenarioNotFoundResult);
                    continue;
                }
                scenarioMethods.Add(methodInfo);
            }

            var scenarioSuiteTestEngine = new ScenarioSuiteTestEngine(scenariosAssemblyLoadContext);
            _runningScenarioSuiteTestEngines.Add(scenarioSuiteTestEngine);

            await scenarioSuiteTestEngine.LoadScenarioSuiteAsync(assemblyScenarioSuiteType, runContext, _cancellationTokenSource.Token);
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                return;
            }
            await scenarioSuiteTestEngine.RunScenariosAsync(scenarioMethods, _cancellationTokenSource.Token);
            await scenarioSuiteTestEngine.CloseApplicationInstancesAsync();
        }

        public virtual async Task ScanForScenariosAsync(IEnumerable<string> assemblyPaths, IMainTestEngineScanContext scanContext)
        {
            var scanningTasks = new List<Task>();

            foreach (var assemblyPath in assemblyPaths) 
            {
                scanningTasks.Add(Task.Run(() =>
                {
                    var scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(assemblyPath);
                    var scenariosAssembly = scenariosAssemblyLoadContext.LoadFromAssemblyPath(assemblyPath);

                    foreach (var assemblyType in scenariosAssembly.GetTypes())
                    {
                        if (assemblyType.FullName == null)
                        {
                            continue;
                        }
                        var scenarioSuiteAttribute = assemblyType.GetCustomAttribute<ScenarioSuiteAttribute>();
                        if (scenarioSuiteAttribute == null)
                        {
                            continue;
                        }

                        var methodInfos = assemblyType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                        foreach (var methodInfo in methodInfos)
                        {
                            var scenarioAttribute = methodInfo.GetCustomAttribute<ScenarioAttribute>();
                            if (scenarioAttribute == null)
                            {
                                continue;
                            }
                            var scenario = new Scenario(scenariosAssembly, assemblyType, methodInfo, scenarioSuiteAttribute, scenarioAttribute);
                            scanContext.OnScenarioFound(scenario);
                        }
                    }
                    scenariosAssemblyLoadContext.Unload();
                }));
            }

            await Task.WhenAll(scanningTasks);
        }

        public async Task StopRunningScenariosAsync()
        {
            _cancellationTokenSource?.Cancel();
            if (_runningScenarioSuiteTestEngines == null || !_runningScenarioSuiteTestEngines.Any())
            {
                return;
            }

            foreach (var runningScenarioSuiteTestEngine in _runningScenarioSuiteTestEngines)
            {
                await runningScenarioSuiteTestEngine.CloseApplicationInstancesAsync();
            }
        }
    }
}
