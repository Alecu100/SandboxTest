using Newtonsoft.Json;
using SandboxTest.Engine.Operations;
using SandboxTest.Engine.Utils;
using SandboxTest.Instance.Hosted;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class HostedInstanceLoop
    {
        private readonly TaskCompletionSource<int> _loopFinishedTaskCompletionSource;
        private IChildTestEngine? _childTestEngine;
        private IHostedInstance? _hostedInstance;
        private Guid _runId;
        private string? _mainPath;
        private string? _assemblySourceName;
        private string? _scenarioSuiteTypeFullName;
        private string? _instanceId;
        private Task? _handleMessagesTask;
        private ScenariosAssemblyLoadContext? _scenariosAssemblyLoadContext;
        private JsonSerializerSettings? _jsonSerializerSettings;

        public HostedInstanceLoop()
        {
            _loopFinishedTaskCompletionSource = new TaskCompletionSource<int>();
        }

        public async Task StartAsync(ScenariosAssemblyLoadContext scenariosAssemblyLoadContext, Dictionary<string, string> hostedDataDictionary)
        {
            try
            {
                var hostedInstanceData = HostedInstanceData.FromDictionary(hostedDataDictionary);
                _runId = hostedInstanceData.RunId;
                _mainPath = hostedInstanceData.MainPath;
                _instanceId = hostedInstanceData.InstanceId;
                _assemblySourceName = hostedInstanceData.AssemblySourceName;
                _scenarioSuiteTypeFullName = hostedInstanceData.ScenarioSuiteTypeFullName;
                _scenariosAssemblyLoadContext = scenariosAssemblyLoadContext;
                _jsonSerializerSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    SerializationBinder = new ScenarioAssemblyLoadContextSerializationBinder(_scenariosAssemblyLoadContext)
                };

                using (var contextualReflectionScope = _scenariosAssemblyLoadContext.EnterContextualReflection())
                {
                    _childTestEngine = new ChildTestEngine(_scenariosAssemblyLoadContext);
                    if (_runId == default || _mainPath == default || _instanceId == default || _assemblySourceName == default || _scenarioSuiteTypeFullName == default)
                    {
                        throw new ArgumentException("All required arguments for application instance runner have not been provided");
                    }
                    var result = await _childTestEngine.LoadInstanceAsync($"{_mainPath}{Path.DirectorySeparatorChar}{_assemblySourceName}", _scenarioSuiteTypeFullName!, _instanceId);
                    if (result.IsSuccesful == false)
                    {
                        throw new InvalidOperationException($"Failed to load instance from scenario suite {_scenarioSuiteTypeFullName} with id {_instanceId}");
                    }
                    _hostedInstance = _childTestEngine.RunningInstance as IHostedInstance;
                }

                _handleMessagesTask = HandleMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                _loopFinishedTaskCompletionSource.SetResult(-1);
                return;
            }

            return;
        }

        ///<inheritdoc/>
        public async Task<int> WaitToStopAsync()
        {
            return await _loopFinishedTaskCompletionSource.Task;
        }

        private async Task HandleMessages()
        {
            if (_childTestEngine == null || _instanceId == null || _hostedInstance == null || _hostedInstance.MessageChannel == null || _scenariosAssemblyLoadContext == null)
            {
                _loopFinishedTaskCompletionSource.SetResult(-1);
                return;
            }

            try
            {
                using (var contextualReflectionScope = _scenariosAssemblyLoadContext.EnterContextualReflection())
                {
                    var messageSink = _hostedInstance.MessageChannel;
                    await messageSink.OpenAsync(_instanceId, _runId, true);
                    while (!_loopFinishedTaskCompletionSource.Task.IsCompleted)
                    {
                        var messageJson = await messageSink.ReceiveMessageAsync();
                        var message = JsonConvert.DeserializeObject<Operation>(messageJson, _jsonSerializerSettings);
                        switch (message)
                        {
                            case RunInstanceOperation runInstanceOperation:
                                var result = await _childTestEngine.RunInstanceAsync(runInstanceOperation.ScenarioSuiteData);
                                await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, _jsonSerializerSettings));
                                break;
                            case StopInstanceOperation stopInstanceOperation:
                                result = await _childTestEngine.StopInstanceAsync(stopInstanceOperation.ScenarioSuiteData);
                                await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, _jsonSerializerSettings));
                                _loopFinishedTaskCompletionSource.SetResult(1);
                                break;
                            case RunScenarioStepOperation runStepOperation:
                                result = await _childTestEngine.RunStepAsync(runStepOperation.StepId, runStepOperation.ScenarioSuiteData, runStepOperation.ScenarioData);
                                await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, _jsonSerializerSettings));
                                break;
                            case ResetInstanceOperation resetInstanceOperation:
                                result = await _childTestEngine.ResetInstanceAsync(resetInstanceOperation.ScenarioSuiteData);
                                await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, _jsonSerializerSettings));
                                break;
                            case LoadScenarioOperation loadScenarioOperation:
                                result = await _childTestEngine.LoadScenarioAsync(loadScenarioOperation.ScenarioMethodName);
                                await messageSink.SendMessageAsync(JsonConvert.SerializeObject(result, _jsonSerializerSettings));
                                break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                _loopFinishedTaskCompletionSource.SetResult(-1);
            }
        }
    }
}
