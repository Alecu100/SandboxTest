using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest
{
    public class ScenarioAttribute : Attribute
    {
        /// <summary>
        /// The name of the scenario
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The category of the scenario
        /// </summary>
        public string Category {  get; set; }
    }
}
