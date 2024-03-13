using SandboxTest.Engine.ChildTestEngine;

namespace SandboxTest.Engine.ApplicationRunner
{
    public class ApplicationInstanceRunner
    {
        private readonly TaskCompletionSource<int> _runFinishedTaskCompletionSource;
        private readonly IChildTestEngine _childTestEngine;
        private Guid _runId;
        private string? _mainPath;
        private string? _assemblySourceName;
        private string? _scenarioSuiteTypeFullName;
        private string? _applicationInstanceId;

        public ApplicationInstanceRunner()
        {
            _childTestEngine = new ChildTestEngine.ChildTestEngine();
            _runFinishedTaskCompletionSource = new TaskCompletionSource<int>();
        }

        public Task InitializeAsync(string[] args)
        {
            try
            {
                _runId = GetArgumentValue<Guid>(args, Constants.RunIdArgument);
                _mainPath = GetArgumentValue<string>(args, Constants.MainPathArgument);
                _applicationInstanceId = GetArgumentValue<string>(args, Constants.ApplicationInstanceIdArgument);
                _assemblySourceName = GetArgumentValue<string>(args, Constants.AssemblySourceNameArgument);
                _scenarioSuiteTypeFullName = GetArgumentValue<string>(args, Constants.ScenarioSuiteTypeFullNameArgument);
                if (_runId == default || _mainPath == default || _applicationInstanceId == default || _assemblySourceName == default || _scenarioSuiteTypeFullName == default)
                {
                    throw new ArgumentException("All required arguments for application instance runner have not been provided");
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
                _runFinishedTaskCompletionSource.SetResult(-1);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public async Task<int> RunAsync()
        {
            return await _runFinishedTaskCompletionSource.Task;
        }

        private TValue? GetArgumentValue<TValue>(string[] args, string name)
        {
            var argument = args.FirstOrDefault(arg => arg.StartsWith($"-{name}="));
            if (argument == null)
            {
                return default;
            }
            var argumentValue = argument.Substring(argument.IndexOf('=') + 1);
            return (TValue)Convert.ChangeType(argumentValue, typeof(TValue));
        }
    }
}
