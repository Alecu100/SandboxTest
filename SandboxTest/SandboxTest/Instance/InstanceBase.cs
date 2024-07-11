using SandboxTest.Scenario;

namespace SandboxTest.Instance
{
    public abstract class InstanceBase : IInstance
    {
        protected readonly List<IController> _controllers = new List<IController>();
        protected readonly string _id;
        protected IRunner? _runner;
        protected List<ScenarioStep> _steps = new List<ScenarioStep>();
        protected int _currentStepIndex;
        protected int? _order;

        protected InstanceBase(string id)
        {
            _id = id;
            _controllers = new List<IController>();
            _steps = new List<ScenarioStep>();
            _currentStepIndex = 0;
            _order = 0;
        }

        /// <summary>
        /// Gets the runner associated to the instance.
        /// </summary>
        public virtual IRunner? Runner { get => _runner; }

        /// <summary>
        /// Gets all the application controllers associated to the application instance
        /// </summary>
        public virtual IReadOnlyList<IController> Controllers { get => _controllers; }

        /// <summary>
        /// Gets all the scenario steps configured for this instance.
        /// </summary>
        public virtual IReadOnlyList<ScenarioStep> Steps { get => _steps; }

        /// <summary>
        /// Gets the id of the instance
        /// </summary>
        public virtual string Id { get => _id; }

        /// <summary>
        /// Returns the current step index used that will be assigned to the next step.
        /// </summary>
        public virtual int CurrentStepIndex { get => _currentStepIndex; }

        /// <summary>
        /// Gets or sets the order in which to start the instance.
        /// </summary>
        public int? Order { get => _order; set => _order = value; }

        /// <summary>
        /// Assigns a specific runner to the instance.
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

        /// <summary>
        /// Adds a controller to the instance.
        /// </summary>
        /// <typeparam name="TController">The controller type, must derive from <see cref="IController"/></typeparam>
        /// <param name="controller">The instance of the controller</param>
        /// <param name="name">The optional name of the controller</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual IInstance AddController<TController>(TController controller) where TController : IController
        {
            if (_controllers.Any(c => c.Name == controller.Name && c.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Already defined an application controller with the name {controller.Name} and the same type.");
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
        /// Resets the current instance, removing all the configured steps for it.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public virtual Task ResetAsync()
        {
            _currentStepIndex = 0;
            _steps.Clear();
            return Task.CompletedTask;
        }
    }
}
