namespace SandboxTest
{
    public class ScenarioStep
    {
        private readonly ScenarioStepId _id;
        private readonly List<Func<Task>> _configuredActions;
        private readonly List<ScenarioStepId> _configuredParentSteps;
        private readonly IApplicationInstance _applicationInstance;

        /// <summary>
        /// Gets the id of the scenario step.
        /// </summary>
        public ScenarioStepId Id { get { return _id; } }

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
        /// <param name="id"></param>
        public ScenarioStep(IApplicationInstance applicationInstance, ScenarioStepId id)
        {
            _id = id;
            _applicationInstance = applicationInstance;
            _configuredParentSteps = new List<ScenarioStepId>();
            _configuredActions = new List<Func<Task>>();
        }

        /// <summary>
        /// Adds a parent step that needs to previously execute before executing the current step
        /// </summary>
        /// <param name="parentStep"></param>
        /// <returns></returns>
        public ScenarioStep WithParentStep(ScenarioStep parentStep)
        {
            _configuredParentSteps.Add(parentStep.Id);
            return this;
        }

        public ScenarioStep InvokeController<TController>(Func<TController, Task> invokeFunc, string? name = default) where TController: IApplicationController
        {
            if (!_applicationInstance.Controllers.Any(controller => ((controller.Name == null && name == null) || (controller.Name != null && controller.Name.Equals(name))) &&
                controller.GetType() == typeof(TController)))
            {
                throw new InvalidOperationException($"Controller with name {name} and type {typeof(TController).Name} not found in the application instance with id {_applicationInstance.Id}");
            }
            _configuredActions.Add(async () =>
            {
                var applicationController = (TController)_applicationInstance.Controllers.First(controller => ((controller.Name == null && name == null) || (controller.Name != null && controller.Name.Equals(name))) &&
                    controller.GetType() == typeof(TController));
                await invokeFunc(applicationController);
            });
            return this;
        }
    }
}
