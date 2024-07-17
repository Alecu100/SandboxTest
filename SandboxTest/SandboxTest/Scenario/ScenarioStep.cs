using System.Xml.Linq;
using SandboxTest.Instance;
using SandboxTest.Internal;

namespace SandboxTest.Scenario
{
    /// <summary>
    /// Represents a scenario step used to executed the configured controller invocations for it when the test is ran.
    /// </summary>
    public class ScenarioStep : IScenarioStepRuntime
    {
        private readonly ScenarioStepId _id;
        private readonly List<Func<IScenarioStepContext, Task>> _configuredActions;
        private readonly List<ScenarioStepId> _previousStepsIds;
        private readonly List<ScenarioStep> _previousSteps;
        private readonly IInstance _applicationInstance;

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
        public ScenarioStep(IInstance applicationInstance)
            : this(applicationInstance, new ScenarioStepId(applicationInstance.Id, applicationInstance.CurrentStepIndex))
        {
        }

        /// <summary>
        /// Creates a new instance of a scenario step.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="name"></param>
        public ScenarioStep(IInstance applicationInstance, string name)
            : this(applicationInstance, new ScenarioStepId(applicationInstance.Id, name))
        {
        }

        /// <summary>
        /// Creates a new instance of a scenario step.
        /// </summary>
        /// <param name="applicationInstance"></param>
        /// <param name="id"></param>
        public ScenarioStep(IInstance applicationInstance, ScenarioStepId id)
        {
            _id = id;
            _applicationInstance = applicationInstance;
            _previousStepsIds = new List<ScenarioStepId>();
            _configuredActions = new List<Func<IScenarioStepContext, Task>>();
            _previousSteps = new List<ScenarioStep>();
        }

        /// <summary>
        /// Adds a step that needs to previously execute before executing the current step
        /// </summary>
        /// <param name="previousStep"></param>
        /// <returns></returns>
        public ScenarioStep AddPreviousStep(ScenarioStep previousStep)
        {
            if (_configuredActions.Count > 0)
            {
                throw new InvalidOperationException("Step already contains controller invocations, please add previous steps before doing any controller invocations.");
            }
            if (previousStep.Id == Id)
            {
                throw new InvalidOperationException("Can't add the current step as a previous step to itself.");
            }
            if (PreviousStepAlreadyAdded(previousStep))
            {
                throw new InvalidOperationException("Previous step to add already contains the current step as a previous step");
            }

            _previousStepsIds.Add(previousStep.Id);
            _previousSteps.Add(previousStep);
            return this;
        }

        public bool PreviousStepAlreadyAdded(ScenarioStep previousStep)
        {
            if (previousStep._previousSteps.Any(step => step.Id == Id))
            {
                return true;
            }
            foreach (var yetPreviousStep in previousStep._previousSteps)
            {
                if (PreviousStepAlreadyAdded(yetPreviousStep))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Uses a controller to call it and execute some actions on the instance.
        /// </summary>
        /// <typeparam name="TController">The controller type to use to do an action on the instance.</typeparam>
        /// <param name="callFunc">The actual action to do on the instace.</param>
        /// <param name="name">Optionally specifies a name to use a specific controller on the instance.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when no controller is found to use.</exception>
        public ScenarioStep UseController<TController>(Func<TController, IScenarioStepContext, Task> callFunc, string? name = default) where TController : IController
        {
            if (!_applicationInstance.Controllers.Any(controller => (controller.Name == null && name == null || controller.Name != null && controller.Name.Equals(name)) &&
                controller.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Controller with name {name} and type {typeof(TController).Name} not found in the application instance with id {_applicationInstance.Id}");
            }
            _configuredActions.Add(async (stepContext) =>
            {
                var applicationController = (TController)_applicationInstance.Controllers.First(controller => (controller.Name == null && name == null || controller.Name != null && controller.Name.Equals(name)) &&
                    controller.GetType() == typeof(TController));
                await callFunc(applicationController, stepContext);
            });
            return this;
        }

        /// <summary>
        /// Uses a controller to call it and execute some actions on the instance.
        /// </summary>
        /// <typeparam name="TController">The controller type to use to do an action on the instance.</typeparam>
        /// <param name="callFunc">The actual action to do on the instace.</param>
        /// <param name="name">Optionally specifies a name to use a specific controller on the instance.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown when no controller is found to use.</exception>
        public ScenarioStep UseController<TController>(Action<TController, IScenarioStepContext> callFunc, string? name = default) where TController : IController
        {
            if (!_applicationInstance.Controllers.Any(controller => (controller.Name == null && name == null || controller.Name != null && controller.Name.Equals(name)) &&
                controller.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Controller with name {name} and type {typeof(TController).Name} not found in the application instance with id {_applicationInstance.Id}");
            }
            _configuredActions.Add((stepContext) =>
            {
                var applicationController = (TController)_applicationInstance.Controllers.First(controller => (controller.Name == null && name == null || controller.Name != null && controller.Name.Equals(name)) &&
                    controller.GetType() == typeof(TController));
                callFunc(applicationController, stepContext);
                return Task.CompletedTask;
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

        /// <inheritdoc/>
        async Task IScenarioStepRuntime.RunAsync(IScenarioStepContext stepContext)
        {
            foreach (var configuredAction in _configuredActions)
            {
                await configuredAction(stepContext);
            }
        }

        /// <summary>
        /// Determines if two steps are equal.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool operator ==(ScenarioStep? first, ScenarioStep? second)
        {
            if (first is null)
            {
                return second is null;
            }
            if (second is null)
            {
                return first is null;
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
            if (first is null)
            {
                return second is not null;
            }
            if (second is null)
            {
                return first is not null;
            }
            return !first.Equals(second);
        }

        /// <summary>
        /// Computes and returns the hashcode of the step id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
