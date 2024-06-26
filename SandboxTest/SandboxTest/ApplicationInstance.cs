﻿namespace SandboxTest
{
    public class ApplicationInstance : IInstance
    {
        protected readonly string _id;
        protected IRunner? _runner;
        protected List<IController> _controllers;
        protected List<ScenarioStep> _steps;
        protected int _currentStepIndex;
        protected IMessageChannel? _messageSink;
        protected bool _isRunning;

        /// <summary>
        /// Creates an empty default application instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static IInstance CreateEmptyInstance(string id)
        {
            return new ApplicationInstance(id);
        }

        public ApplicationInstance(string id)
        {
            _id = id;
            _controllers = new List<IController>();
            _steps = new List<ScenarioStep>();
            _currentStepIndex = 0;
        }

        /// <summary>
        /// Gets the application runner associated to the application instance that actually starts the application instance.
        /// </summary>
        public virtual IRunner? Runner { get => _runner; }

        /// <summary>
        /// Gets all the application controllers associated to the application instance
        /// </summary>
        public virtual IReadOnlyList<IController> Controllers { get => _controllers; }

        /// <summary>
        /// Gets all the scenario steps configured for this application instance.
        /// </summary>
        public virtual IReadOnlyList<ScenarioStep> Steps { get => _steps; }

        /// <summary>
        /// Gets the id of the application instance
        /// </summary>
        public virtual string Id { get => _id; }

        /// <summary>
        /// Returns the current step index used that will be assigned to the next step.
        /// </summary>
        public virtual int CurrentStepIndex { get => _currentStepIndex; }

        /// <summary>
        /// Returns the 
        /// </summary>
        public virtual IMessageChannel MessageSink
        {
            get
            {
                if (_messageSink == null)
                {
                    _messageSink = new ApplicationMessageChannel();
                }
                return _messageSink;
            }
        }

        /// <summary>
        /// Assigns a specific runner to the application instance.
        /// </summary>
        /// <typeparam name="TRunner"></typeparam>
        /// <param name="applicationRunner"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual IInstance UseRunner<TRunner>(TRunner applicationRunner) where TRunner : IRunner
        {
            if (_runner != null) 
            {
                throw new InvalidOperationException("Application instance already has a runner assigned.");
            }
            _runner = applicationRunner;

            return this;
        }

        public virtual IInstance AddController<TController>(TController controller, string? name = default) where TController : IController
        {
            if (_controllers.Any(c => c.Name == name && c.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Already defined an application controller with the name {name} and the same type.");
            }
            _controllers.Add(controller);

            return this;
        }

        /// <summary>
        /// Adds a new step for the current instance.
        /// </summary>
        /// <returns></returns>
        public virtual ScenarioStep AddStep()
        {
            var scenarioStep = new ScenarioStep(this);
            _currentStepIndex++;
            _steps.Add(scenarioStep);
            return scenarioStep;
        }

        /// <summary>
        /// Adds a step with a dedicated name to be more explicit and readable.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual ScenarioStep AddStep(string name)
        {
            var scenarioStep = new ScenarioStep(this, name);
            _currentStepIndex++;
            _steps.Add(scenarioStep);
            return scenarioStep;
        }

        /// <summary>
        /// Starts the current application instance, running all the required steps in order to run it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task StartAsync()
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
            await _runner.BuildAsync();
            foreach (var controller in _controllers)
            {
                await controller.BuildAsync(this);
            }
            await _runner.ConfigureRunAsync();
            await _runner.RunAsync();
            _isRunning = true;
        }

        /// <summary>
        /// Stops the current application instance.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task StopAsync()
        {
            if (_runner == null)
            {
                throw new InvalidOperationException("Application instance has no assigned runner.");
            }
            await _runner.StopAsync();
        }

        /// <summary>
        /// Resets the current application instance, removing all the configured steps for it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual async Task ResetAsync()
        {
            if (_runner == null)
            {
                throw new InvalidOperationException("Application instance has no assigned runner.");
            }
            if (_isRunning)
            {
                await _runner.ResetAsync();
                foreach (var controller in _controllers)
                {
                    await controller.ResetAsync(this);
                }
            }
            _currentStepIndex = 0;
            _steps.Clear();
        }
    }
}
