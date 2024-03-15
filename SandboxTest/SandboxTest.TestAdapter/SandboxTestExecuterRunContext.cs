using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using SandboxTest.Engine;
using SandboxTest.Engine.MainTestEngine;
using System.Diagnostics;

namespace SandboxTest.TestAdapter
{
    public class SandboxTestExecuterRunContext : IMainTestEngineRunContext
    {
        private readonly IRunContext? _runContext;
        private readonly IFrameworkHandle? _frameworkHandle;

        public SandboxTestExecuterRunContext(IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            _runContext = runContext;
            _frameworkHandle = frameworkHandle;
        }

        public bool IsBeingDebugged { get => _runContext?.IsBeingDebugged ?? false; }

        public Task<Process> LaunchProcessAsync(string filePath, bool debug, string? workingDirectory, string? arguments, IDictionary<string, string?>? environmentVariables)
        {
            if (_frameworkHandle == null)
            {
                throw new InvalidOperationException("Can't start process without references to IFrameworkHandle");
            }

            if (debug)
            {
                var processId = _frameworkHandle.LaunchProcessWithDebuggerAttached(filePath, workingDirectory, arguments, environmentVariables);
                return Task.FromResult(Process.GetProcessById(processId));
            }

            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = filePath;
            processStartInfo.Arguments = arguments;
            processStartInfo.WorkingDirectory = workingDirectory;
            if (environmentVariables != null)
            {
                foreach (var environmentVariable in environmentVariables) 
                {
                    processStartInfo.EnvironmentVariables.Add(environmentVariable.Key, environmentVariable.Value);
                }
            }
            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InvalidOperationException($"Failed to start process for path {filePath}");
            }
            return Task.FromResult(process);
        }
        
        /// <summary>
        /// Sends a test log message to the visual studio test host.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task LogMessage(LogLevel logLevel, string message)
        {
            _frameworkHandle?.SendMessage((TestMessageLevel)(int)logLevel, message);
            return Task.CompletedTask;
        }

        public Task OnScenarioRanAsync(ScenarioRunResult scenarioRunResult)
        {
            if (_frameworkHandle == null)
            {
                return Task.CompletedTask;
            }
            _frameworkHandle.RecordResult(scenarioRunResult.ConvertToTestResult());
            return Task.CompletedTask;
        }

        public Task OnScenarioRunningAsync(Scenario scenario)
        {
            if (_frameworkHandle == null)
            {
                return Task.CompletedTask;
            }
            _frameworkHandle.RecordStart(scenario.ConvertToTestCase());
            return Task.CompletedTask;
        }
    }
}
