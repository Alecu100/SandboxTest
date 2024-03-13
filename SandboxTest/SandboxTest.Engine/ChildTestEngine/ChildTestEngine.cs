﻿using SandboxTest.Engine.Operations;
using System.Collections.Generic;
using System.Reflection;

namespace SandboxTest.Engine.ChildTestEngine
{
    public class ChildTestEngine : IChildTestEngine
    {
        private Type? _scenarioSuiteType;
        private ScenariosAssemblyLoadContext? _scenariosAssemblyLoadContext;
        private Assembly? _scenarioSuiteAssembly;
        private IApplicationInstance? _runningInstance;
        private object? _scenarioSuiteInstance;

        public IApplicationInstance? RunningInstance { get => _runningInstance; }

        public virtual Task<OperationResult> LoadScenarioAsync(string scenarioMethodName)
        {
            throw new NotImplementedException();
        }

        public async virtual Task<OperationResult> ResetApplicationInstanceAsync()
        {
            if (_runningInstance == null)
            {
                return new OperationResult(false, "No application instance running");
            }
            await _runningInstance.ResetAsync();

            return new OperationResult(true);
        }

        public async virtual Task<OperationResult> RunApplicationInstanceAsync(string sourceAssemblyNameFulPath, string scenarioSuiteTypeFullName, string applicationInstanceId)
        {
            try
            {
                _scenariosAssemblyLoadContext = new ScenariosAssemblyLoadContext(sourceAssemblyNameFulPath);
                _scenarioSuiteAssembly = _scenariosAssemblyLoadContext.LoadFromAssemblyPath(sourceAssemblyNameFulPath);
                foreach (var assemblyType in _scenarioSuiteAssembly.GetTypes())
                {
                    if (assemblyType.FullName == scenarioSuiteTypeFullName)
                    {
                        _scenarioSuiteType = assemblyType;
                        break;
                    }
                }

                if (_scenarioSuiteType == null)
                {
                    throw new InvalidOperationException($"Could not find scenario suite type {scenarioSuiteTypeFullName} to load application instance from");
                }
                if (_scenarioSuiteType.GetConstructors().All(constructor => constructor.GetParameters().Length > 0))
                {
                    throw new InvalidOperationException($"Scenario suite type {scenarioSuiteTypeFullName} does not contain a constructor without parameters");
                }
                _scenarioSuiteInstance = Activator.CreateInstance(_scenarioSuiteType);

                var allFields = new List<FieldInfo>();
                allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.Public));
                allFields.AddRange(_scenarioSuiteType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic));
                foreach (var field in allFields) 
                {
                    if (field.FieldType.IsAssignableTo(typeof(IApplicationInstance)))
                    {
                        var applicationInstance = field.GetValue(_scenarioSuiteInstance) as IApplicationInstance;
                        if (applicationInstance != null && applicationInstance.Id == applicationInstanceId)
                        {
                            _runningInstance = applicationInstance;
                            break;
                        }
                    }
                }

                if (_runningInstance == null)
                {
                    throw new InvalidOperationException($"Could not find an application instance with id {applicationInstanceId} in scenario suite type {_scenarioSuiteType.FullName}");
                }
                if (_runningInstance.MessageSink == null)
                {
                    throw new InvalidOperationException($"Application instance with id {applicationInstanceId} has no message sink assigned");
                }
                await _runningInstance.StartAsync();
                return new OperationResult(true);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
                return new OperationResult(false, ex.ToString());
            }
        }

        public async virtual Task<OperationResult> RunStepAsync(ScenarioStepId stepId, ScenarioStepContext stepContext)
        {
            if (_runningInstance == null)
            {
                return new RunScenarioStepOperationResult(false, stepContext, "No application instance running");
            }
            var step = _runningInstance.Steps.FirstOrDefault(step => step.Id.ApplicationInstanceId == _runningInstance.Id && ((step.Id.Name != null && stepId.Name == step.Id.Name) || (step.Id.StepIndex == stepId.StepIndex)));
            if (step == null)
            {
                return new RunScenarioStepOperationResult(false, stepContext, "Step not found");
            }
            try
            {
                await step.RunAsync(stepContext);
                return new RunScenarioStepOperationResult(true, stepContext);
            }
            catch (Exception ex)
            {
                return new RunScenarioStepOperationResult(false, stepContext, $"Error running step {ex}");
            }
        }

        public Task<OperationResult> CloseApplicationInstanceAsync()
        {
            throw new NotImplementedException();
        }
    }
}
