﻿namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarionContainer
    {
        private readonly IApplicationInstance _applicationInstance1 = 
            new ApplicationInstance(Guid.NewGuid().ToString());

        private readonly IApplicationInstance _applicationInstance2 =
            new ApplicationInstance(Guid.NewGuid().ToString());


        [Scenario]
        public void TestScenarioMethod()
        {

        }
    }
}
