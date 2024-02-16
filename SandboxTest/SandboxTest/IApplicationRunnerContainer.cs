using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest
{
    public interface IApplicationRunnerContainer
    {
        IApplicationRunner? Runner { get; }

        ApplicationInstance AssingRunner<TRunner>(TRunner applicationRunner) where TRunner : IApplicationRunner;
    }
}
