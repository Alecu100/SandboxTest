using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SandboxTest.TestAdapter
{
    public class SandboxTestExecuter : ITestExecutor
    {
        public void Cancel()
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
        {
            if (!Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }
    }
}
