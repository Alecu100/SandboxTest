using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Internal;

namespace SandboxTest.Instance
{
    /// <summary>
    /// Controller that allows manually running and stopping an instance by stopping and starting its associated runner.
    /// </summary>
    public class RunnerLifecycleController : ControllerBase, IRuntimeContextAccessor
    {
        private IRuntimeContext? _runtimeContext;

        public RunnerLifecycleController(string? name) : base(name)
        {
        }

        /// <summary>
        /// Starts the instance by running the runner.
        /// </summary>
        /// <returns></returns>
        public async Task RunRunnerAsync()
        {
            var allInstancesToRun = new List<object>();
            allInstancesToRun.AddRange(_runtimeContext!.Controllers);
            allInstancesToRun.Add(_runtimeContext!.Runner);
            await _runtimeContext.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runtimeContext.Runner.RunAsync, new object[] { _runtimeContext.Runner });
        }

        /// <summary>
        /// Stops the instance by stopping the runner.
        /// </summary>
        /// <returns></returns>
        public async Task StopRunnerAsync()
        {
            var allInstancesToRun = new List<object>();
            allInstancesToRun.AddRange(_runtimeContext!.Controllers);
            allInstancesToRun.Add(_runtimeContext!.Runner);
            await _runtimeContext.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runtimeContext.Runner.StopAsync, new object[] { _runtimeContext.Runner });
        }


        /// <summary>
        /// Resets the instance by resetting the runner.
        /// </summary>
        /// <returns></returns>
        public async Task ResetRunnerAsync()
        {
            var allInstancesToRun = new List<object>();
            allInstancesToRun.AddRange(_runtimeContext!.Controllers);
            allInstancesToRun.Add(_runtimeContext!.Runner);
            await _runtimeContext.ExecuteAttachedMethodsChain(allInstancesToRun, new[] { AttachedMethodType.RunnerToRunner, AttachedMethodType.ControllerToRunner }, _runtimeContext.Runner.ResetAsync, new object[] { _runtimeContext.Runner });
        }

        void IRuntimeContextAccessor.InitializeContext(IRuntimeContext context)
        {
            _runtimeContext = context;
        }
    }
}
