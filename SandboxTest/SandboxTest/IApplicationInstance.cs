using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest
{
    /// <summary>
    /// Represents an instance of a application that contains some defined steps, a runner and various assigned controllers
    /// </summary>
    public interface IApplicationInstance: IApplicationControllerContainer, IApplicationRunnerContainer, IScenarioStepContainer
    {
        string Id { get; }
    }
}
