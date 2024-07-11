using SandboxTest.Engine.MainTestEngine;
using SandboxTest.Instance.Hosted;
using SandboxTest.Scenario;
using System.Diagnostics;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class HostedInstanceContext : IHostedInstanceContext
    {
        private readonly IMainTestEngineRunContext _mainTestEngineRunContext;

        private readonly ScenarioSuiteData _scenarioSuiteData;

        /// <summary>
        /// Creates a new instance of <see cref="HostedInstanceContext"/>
        /// </summary>
        /// <param name="mainTestEngineRunContext"></param>
        /// <param name="scenarioSuiteData"></param>
        public HostedInstanceContext(IMainTestEngineRunContext mainTestEngineRunContext, ScenarioSuiteData scenarioSuiteData)
        {
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteData = scenarioSuiteData;
        }

        /// <inheritdoc/>
        public bool IsBeingDebugged { get => _mainTestEngineRunContext.IsBeingDebugged; }

        /// <inheritdoc/>
        public ScenarioSuiteData ScenarioSuiteData { get => _scenarioSuiteData; }

        public async Task<Process> LaunchProcessAsync(string filePath, bool debug, string? workingDirectory = null, string? arguments = null, IDictionary<string, string?>? environmentVariables = null, CancellationToken token = default)
        {
            return await _mainTestEngineRunContext.LaunchProcessAsync(filePath, debug, workingDirectory, arguments, environmentVariables, token);
        }
    }
}
