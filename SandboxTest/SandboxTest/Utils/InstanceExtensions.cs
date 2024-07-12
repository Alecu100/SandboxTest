using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.Utils
{
    /// <summary>
    /// Convenience extensions for <see cref="IInstance"/>
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Disables automatic running of an instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public static IInstance DisableAutoRun(this IInstance instance)
        {
            instance.Order = null;
            return instance;
        }

        /// <summary>
        /// Adds a step to an instance along with a previous step for it.
        /// </summary>
        /// <param name="instance">The instance to which to add the new step.</param>
        /// <param name="previousStep">The previous step to set for the added step.</param>
        /// <returns></returns>
        public static ScenarioStep AddStep(this IInstance instance, ScenarioStep previousStep)
        {
            var addedStep = instance.AddStep();
            addedStep.AddPreviousStep(previousStep);
            return addedStep;
        }

        /// <summary>
        /// Adds an controller of type <see cref="RunnerController"/> to the given instance.
        /// Only one can be assigned per instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddRunnerController(this IInstance instance)
        {
            var existingRunnerController = instance.Controllers.FirstOrDefault(controller => controller.GetType() == typeof(RunnerController));
            if (existingRunnerController != null)
            {
                throw new InvalidOperationException("Instance already has a runner controller assigned");
            }

            var runnerController = new RunnerController();
            instance.AddController(runnerController);
            return instance;
        }
    }
}
