using System.Reflection;

namespace SandboxTest.Engine.MainTestEngine
{
    public class ScenarioSuiteTestEngine : IScenarioSuiteTestEngine
    {
        protected List<Func<ScenarioRunResult, Task>> _onScenarioRanAsyncCallbacks;
        protected List<Func<Scenario, Task>> _onScenarioRunningAsyncCallbacks;
        protected List<ScenarioSuiteTestEngineApplicationInstance> _applicationInstances;

        public ScenarioSuiteTestEngine()
        {
            _onScenarioRanAsyncCallbacks = new List<Func<ScenarioRunResult, Task>>();
            _onScenarioRunningAsyncCallbacks = new List<Func<Scenario, Task>>();
            _applicationInstances = new List<ScenarioSuiteTestEngineApplicationInstance>();
        }

        public Task CloseApplicationInstancesAsync()
        {
            throw new NotImplementedException();
        }

        public virtual Task LoadScenarioSuiteAsync(Type scenarioSuiteType, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public virtual void OnScenarionRan(Func<ScenarioRunResult, Task> onScenarioRanAsyncCallback)
        {
            _onScenarioRanAsyncCallbacks.Add(onScenarioRanAsyncCallback);
        }

        public virtual void OnScenarioRunning(Func<Scenario, Task> onScenarioRunningAsyncCallback)
        {
            _onScenarioRunningAsyncCallbacks.Add(onScenarioRunningAsyncCallback);
        }

        public virtual Task RunScenariosAsync(List<MethodInfo> scenarionMethods, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
