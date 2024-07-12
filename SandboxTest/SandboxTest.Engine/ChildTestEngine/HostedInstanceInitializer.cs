using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using SandboxTest.Instance.Hosted;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class HostedInstanceInitializer : IHostedInstanceInitializer
    {
        private readonly TaskCompletionSource<int> _runFinishedTaskCompletionSource;
        private IChildTestEngine? _childTestEngine;
        private IHostedInstance? _hostedInstance;
        private Guid _runId;
        private string? _mainPath;
        private string? _assemblySourceName;
        private string? _scenarioSuiteTypeFullName;
        private string? _applicationInstanceId;
        private Task? _handleMessagesTask;

        public HostedInstanceInitializer()
        {
            _runFinishedTaskCompletionSource = new TaskCompletionSource<int>();
        }

        ///<inheritdoc/>
        public async Task InitalizeAsync(HostedInstanceData hostedInstanceData)
        {
            try
            {
                _runId = hostedInstanceData.RunId;
                _mainPath = hostedInstanceData.MainPath;
                _applicationInstanceId = hostedInstanceData.ApplicationInstanceId;
                _assemblySourceName = hostedInstanceData.AssemblySourceName;
                _scenarioSuiteTypeFullName = hostedInstanceData.ScenarioSuiteTypeFullName;
                _childTestEngine = new ChildTestEngine(new ScenariosAssemblyLoadContext($"{_mainPath}\\{_assemblySourceName}"));
                if (_runId == default || _mainPath == default || _applicationInstanceId == default || _assemblySourceName == default || _scenarioSuiteTypeFullName == default)
                {
                    throw new ArgumentException("All required arguments for application instance runner have not been provided");
                }
                var result = await _childTestEngine.LoadInstanceAsync($"{_mainPath}\\{_assemblySourceName}", _scenarioSuiteTypeFullName!, _applicationInstanceId);
                if (result.IsSuccesful == false)
                {
                    throw new InvalidOperationException($"Failed to load instance from scenario suite {_scenarioSuiteTypeFullName} with id {_applicationInstanceId}");
                }
                _hostedInstance = _childTestEngine.RunningInstance as IHostedInstance;
                _handleMessagesTask = Task.Run(HandleMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _runFinishedTaskCompletionSource.SetResult(-1);
                return;
            }

            return;
        }

        ///<inheritdoc/>
        public async Task<int> WaitToFinishAsync()
        {
            return await _runFinishedTaskCompletionSource.Task;
        }

        private async Task HandleMessages()
        {
            if (_childTestEngine == null || _applicationInstanceId == null || _hostedInstance == null || _hostedInstance.MessageChannel == null)
            {
                _runFinishedTaskCompletionSource.SetResult(-1);
                return;
            }

            try
            {
                var messageSink = _hostedInstance.MessageChannel;
                await messageSink.StartAsync(_applicationInstanceId, _runId, true);
                while (!_runFinishedTaskCompletionSource.Task.IsCompleted)
                {
                    var messageJson = await messageSink.ReceiveMessageAsync();
                    var message = JsonConvert.DeserializeObject<Operation>(messageJson, JsonUtils.JsonSerializerSettings);
                    switch (message)
                    {
                        case RunInstanceOperation runInstanceOperation:
                            var result = await _childTestEngine.RunInstanceAsync(runInstanceOperation.ScenarioSuiteData);
                            await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, JsonUtils.JsonSerializerSettings));
                            break;
                        case StopInstanceOperation stopInstanceOperation:
                            result = await _childTestEngine.StopInstanceAsync(stopInstanceOperation.ScenarioSuiteData);
                            await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, JsonUtils.JsonSerializerSettings));
                            _runFinishedTaskCompletionSource.SetResult(1);
                            break;
                        case RunScenarioStepOperation runStepOperation:
                            result = await _childTestEngine.RunStepAsync(runStepOperation.StepId, runStepOperation.ScenarioSuiteData, runStepOperation.ScenarioData);
                            await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, JsonUtils.JsonSerializerSettings));
                            break;
                        case ResetInstanceOperation resetInstanceOperation:
                            result = await _childTestEngine.ResetInstanceAsync(resetInstanceOperation.ScenarioSuiteData);
                            await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, JsonUtils.JsonSerializerSettings));
                            break;
                        case LoadScenarioOperation loadScenarioOperation:
                            result = await _childTestEngine.LoadScenarioAsync(loadScenarioOperation.ScenarioMethodName);
                            await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, JsonUtils.JsonSerializerSettings));
                            break;
                    }
                }
            }
            catch (Exception)
            {
                _runFinishedTaskCompletionSource.SetResult(-1);
            }
        }
    }
}
