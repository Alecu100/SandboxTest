using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest
{
    public interface IApplicationControllerContainer
    {
        IReadOnlyList<IApplicationController> Controllers { get; }

        ApplicationInstance AssignController<TApplicationController>(TApplicationController controller, string? name = null) where TApplicationController : IApplicationController;
    }
}
