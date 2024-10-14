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
        private readonly string? _packageFolder;

        /// <summary>
        /// Creates a new instance of <see cref="HostedInstanceContext"/>
        /// </summary>
        /// <param name="mainTestEngineRunContext"></param>
        /// <param name="scenarioSuiteData"></param>
        public HostedInstanceContext(IMainTestEngineRunContext mainTestEngineRunContext, ScenarioSuiteData scenarioSuiteData, string? packageFolder = null)
        {
            _mainTestEngineRunContext = mainTestEngineRunContext;
            _scenarioSuiteData = scenarioSuiteData;
            _packageFolder = packageFolder;
        }

        /// <inheritdoc/>
        public bool IsBeingDebugged { get => _mainTestEngineRunContext.IsBeingDebugged; }

        /// <summary>
        /// Gets the package folder when the hosted instance has <see cref="IHostedInstance.IsPackaged"/> set to true.
        /// </summary>
        public string? PackageFolder { get => _packageFolder; }

        /// <inheritdoc/>
        public ScenarioSuiteData ScenarioSuiteData { get => _scenarioSuiteData; }

        public async Task<Process> LaunchProcessAsync(string filePath, bool debug, string? workingDirectory = null, string? arguments = null, IDictionary<string, string?>? environmentVariables = null, CancellationToken token = default)
        {
            return await _mainTestEngineRunContext.LaunchProcessAsync(filePath, debug, workingDirectory, arguments, environmentVariables, token);
        }
    }
}
