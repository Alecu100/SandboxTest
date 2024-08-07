﻿using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using SandboxTest.Engine;

namespace SandboxTest.TestAdapter
{
    public static class ScenarioExtensions
    {
        /// <summary>
        /// Converts a scenario to a test case
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public static TestCase ConvertToTestCase(this Engine.Scenario scenario)
        {
            var testCase = new TestCase($"{scenario.ScenarioSuitTypeFullName}.{scenario.ScenarioMethodName}", new Uri("executor://sandboxtest.testadapter"), scenario.ScenarioSourceAssembly);
            return testCase;
        }

        /// <summary>
        /// Converts a test case to a scenario.
        /// </summary>
        /// <param name="testCase"></param>
        /// <returns></returns>
        public static Engine.Scenario ConvertToScenario(this TestCase testCase)
        {
            var methodName = testCase.FullyQualifiedName.Substring(testCase.FullyQualifiedName.LastIndexOf('.') + 1);
            var scenarioSuitTypeFullName = testCase.FullyQualifiedName.Substring(0, testCase.FullyQualifiedName.LastIndexOf("."));
            return new Engine.Scenario(testCase.Source, scenarioSuitTypeFullName, methodName);
        }

        public static TestResult ConvertToTestResult(this ScenarioRunResult scenarioRunResult)
        {
            var testResult = new TestResult(scenarioRunResult.Scenario.ConvertToTestCase());
            testResult.Duration = scenarioRunResult.Duration;
            testResult.StartTime = scenarioRunResult.StartTime;

            switch (scenarioRunResult.Result) 
            {
                case ScenarioRunResultType.Successful:
                    testResult.Outcome = TestOutcome.Passed; 
                    break;
                case ScenarioRunResultType.Failed:
                    testResult.Outcome = TestOutcome.Failed;
                    testResult.ErrorMessage = scenarioRunResult.ErrorMessage;
                    break;
                case ScenarioRunResultType.NotFound:
                    testResult.Outcome = TestOutcome.NotFound;
                    break;
                default:
                    testResult.Outcome = TestOutcome.None; 
                    break;
            }

            return testResult;
        }
    }
}
