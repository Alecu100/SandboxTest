using System.Xml.Linq;

namespace SandboxTest
{
    public class ScenarioStep
    {
        private readonly ScenarioStepId _id;
        private readonly List<Func<ScenarioStepContext, Task>> _configuredActions;
        private readonly List<ScenarioStepId> _previousStepsIds;
        private readonly IApplicationInstance _applicationInstance;

        /// <summary>
        /// Gets the id of the scenario step.
        /// </summary>
        public ScenarioStepId Id { get { return _id; } }

        /// <summary>
        /// Returns all the configured previous steps ids to run before the current step.
        /// </summary>
        public IReadOnlyList<ScenarioStepId> PreviousStepsIds { get { return _previousStepsIds; } }

        /// <summary>
        /// Creates a new instance of a scenario step.
        /// </summary>
        /// <param name="applicationInstance"></param>
        public ScenarioStep(IApplicationInstance applicationInstance) 
            : this(applicationInstance, new ScenarioStepId(applicationInstance.Id, applicationInstance.CurrentStepIndex))
        {
        }

        /// <summary>
        /// Creates a new instance of a scenario step.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        public ScenarioStep(IApplicationInstance applicationInstance, string name)
            : this(applicationInstance, new ScenarioStepId(applicationInstance.Id, name))
        {
        }

        /// <summary>
        /// Creates a new instance of a scenario step.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="id"></param>
        public ScenarioStep(IApplicationInstance applicationInstance, ScenarioStepId id)
        {
            _id = id;
            _applicationInstance = applicationInstance;
            _previousStepsIds = new List<ScenarioStepId>();
            _configuredActions = new List<Func<ScenarioStepContext, Task>>();
        }

        /// <summary>
        /// Adds a step that needs to previously execute before executing the current step
        /// </summary>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        public ScenarioStep ConfigurePreviousStep(ScenarioStep previousStep)
        {
            if (_configuredActions.Count > 0)
            {
                throw new Exception("Step already contains controller invocations, please add previous steps before doing any controller invocations.");
            }

            _previousStepsIds.Add(previousStep.Id);
            return this;
        }

        public async Task RunAsync(ScenarioStepContext stepContext)
        {
            foreach (var configuredAction in _configuredActions)
            {
                await configuredAction(stepContext);
            }
        }

        public ScenarioStep InvokeController<TController>(Func<TController, ScenarioStepContext, Task> invokeFunc, string? name = default) where TController: IApplicationController
        {
            if (!_applicationInstance.Controllers.Any(controller => ((controller.Name == null && name == null) || (controller.Name != null && controller.Name.Equals(name))) &&
                controller.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Controller with name {name} and type {typeof(TController).Name} not found in the application instance with id {_applicationInstance.Id}");
            }
            _configuredActions.Add(async (ScenarioStepContext context) =>
            {
                var applicationController = (TController)_applicationInstance.Controllers.First(controller => ((controller.Name == null && name == null) || (controller.Name != null && controller.Name.Equals(name))) &&
                    controller.GetType() == typeof(TController));
                await invokeFunc(applicationController, context);
            });
            return this;
        }

        /// <summary>
        /// Determines if the current step is equal to the other step passed as parameter
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is not ScenarioStep || obj == null || Id == null)
            {
                return false;
            }

            var otherScenarioStep = (ScenarioStep)obj;
            return Id.Equals(otherScenarioStep.Id);
        }

        /// <summary>
        /// Converts a step to a string representation.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _id?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Determines if two steps are equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(ScenarioStep? first, ScenarioStep? second)
        {
            if (first == null)
            {
                return second == null;
            }
            if (second == null)
            {
                return first == null;
            }
            return first.Equals(second);
        }

        /// <summary>
        /// Determines if two steps are not equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator !=(ScenarioStep? first, ScenarioStep? second)
        {
            if (first == null)
            {
                return second != null;
            }
            if (second == null)
            {
                return first != null;
            }
            return !first.Equals(second);
        }
    }
}
