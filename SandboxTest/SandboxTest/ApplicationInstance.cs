namespace SandboxTest
{
    public class ApplicationInstance : IApplicationInstance
    {
        private readonly string _id;
        private IApplicationRunner? _runner;
        private List<IApplicationController> _controllers;
        private List<ScenarioStep> _steps;
        private int _currentStepIndex;

        public ApplicationInstance(string id)
        {
            _id = id;
            _controllers = new List<IApplicationController>();
            _steps = new List<ScenarioStep>();
            _currentStepIndex = 0;
        }

        /// <summary>
        /// Gets the application runner associated to the application instance that actually starts the application instance.
        /// </summary>
        public IApplicationRunner? Runner { get => _runner; }

        /// <summary>
        /// Gets all the application controllers associated to the application instance
        /// </summary>
        public IReadOnlyList<IApplicationController> Controllers { get => _controllers; }

        /// <summary>
        /// Gets all the scenario steps configured for this application instance.
        /// </summary>
        public IReadOnlyList<ScenarioStep> Steps { get => _steps; }

        /// <summary>
        /// Gets the id of the application instance
        /// </summary>
        public string Id { get => _id; }

        /// <summary>
        /// Returns the current step index used that will be assigned to the next step.
        /// </summary>
        public int CurrentStepIndex { get => _currentStepIndex; }

        /// <summary>
        /// Assigns a specific runner to the application instance.
        /// </summary>
        /// <typeparam name="TRunner"></typeparam>
        /// <param name="applicationRunner"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ApplicationInstance AssingRunner<TRunner>(TRunner applicationRunner) where TRunner : IApplicationRunner
        {
            if (_runner != null) 
            {
                throw new InvalidOperationException("Application instance already has a runner assigned.");
            }
            _runner = applicationRunner;

            return this;
        }

        public ApplicationInstance AssignController<TApplicationController>(TApplicationController controller, string? name = default) where TApplicationController : IApplicationController
        {
            if (_controllers.Any(c => c.Name == name)) 
            {
                throw new InvalidOperationException($"Already defined an application controller with the name {name}.");
            }
            _controllers.Add(controller);

            return this;
        }

        /// <summary>
        /// Adds a new step for the current instance.
        /// </summary>
        /// <returns></returns>
        public ScenarioStep AddStep()
        {
            var scenarioStep = new ScenarioStep(this);
            _currentStepIndex++;
            return scenarioStep;
        }

        public async Task Start(string[] args)
        {
            if (_runner == null)
            {
                throw new InvalidOperationException("Application instance has no assigned runner.");
            }

            await _runner.ConfigureBuildAsync();
            foreach (var controller in _controllers)
            {
                await controller.ConfigureBuildAsync(this);
            }
            foreach (var controller in _controllers)
            {
                await controller.BuildAsync(this);
            }
            await _runner.ConfigureRunAsync();
            await _runner.RunAsync();
        }

        public async Task StopAsync()
        {
            if (_runner == null)
            {
                throw new InvalidOperationException("Application instance has no assigned runner.");
            }
            await _runner.StopAsync();
        }
    }
}
