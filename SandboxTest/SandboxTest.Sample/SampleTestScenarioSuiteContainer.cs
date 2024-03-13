namespace SandboxTest.Sample
{
    [ScenarioSuite]
    public class SampleTestScenarioSuiteContainer
    {
        private readonly IApplicationInstance _applicationInstance1 = 
            new ApplicationInstance("Instance1");

        private readonly IApplicationInstance _applicationInstance2 =
            new ApplicationInstance("Instance2");


        [Scenario]
        public void TestScenarioMethod()
        {

        }
    }
}
