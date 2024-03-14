namespace SandboxTest
{
    public interface IScenarioStepContainer
    {
        IReadOnlyList<ScenarioStep> Steps { get; }

        int CurrentStepIndex { get; }

        ScenarioStep AddStep();
        ScenarioStep AddStep(string name);
    }
}
