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

        /// <summary>
        /// Resets the instance of the application so that it can be reused for another test scenario.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();
    }
}
