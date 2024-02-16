using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest
{
    public interface IScenarioStepContainer
    {
        IReadOnlyList<ScenarioStep> Steps { get; }

        int CurrentStepIndex { get; }

        ScenarioStep AddStep();
    }
}
