﻿using System.Collections;
using System.ComponentModel;

namespace SandboxTest.Instance.Hosted
{
    /// <summary>
    /// Represents required data required for a <see cref="IHostedInstance"/> to start.
    /// </summary>
    public class HostedInstanceData
    {
        public const string RunIdArg = "run-id";

        public const string RunIdEnv = "Sandbox_Test_Run_id";

        public const string MainPathArg = "main-path";

        public const string MainPathEnv = "Sandbox_Test_Main_Path";

        public const string AssemblySourceNameArg = "assembly-source-name";

        public const string AssemblySourceNameEnv = "Sandbox_Test_Assembly_Source_Name";

        public const string ScenarioSuiteTypeFullNameArg = "scenario-suite-type-full-name";

        public const string ScenarioSuiteTypeFullNameEnv = "Sandbox_Test_Scenario_Suite_Type_Full_Name";

        public const string InstanceIdArg = "instance-id";

        public const string InstanceIdEnv = "Sandbox_Test_Instance_Id";

        public const string HostedInstanceInitializerTypeFullNameArg = "hosted-instance-initializer-type-full-name";

        public const string HostedInstanceInitializerTypeFullNameEnv = "Sandbox_Test_Hosted_Instance_Initializer_Type_Full_Name";

        public const string HostedInstanceInitializerAssemblyFullNameArg = "hosted-instance-initializer-assembly-full-name";

        public const string HostedInstanceInitializerAssemblyFullNameEnv = "Sandbox_Test_Hosted_instance_Initializer_Assembly_Full_name";

        /// <summary>
        /// Used internally by the test engine.
        /// </summary>
        public Guid RunId { get; set; }

        /// <summary>
        /// Used internally by the test engine.
        /// </summary>
        public string MainPath { get; set; } = string.Empty;

        /// <summary>
        /// Used internally by the test engine.
        /// </summary>
        public string AssemblySourceName { get; set; } = string.Empty;

        /// <summary>
        /// Used internally by the test engine.
        /// </summary>
        public string ScenarioSuiteTypeFullName { get; set; } = string.Empty;

        /// <summary>
        /// Used internally by the test engine.
        /// </summary>
        public string InstanceId { get; set; } = string.Empty;

        /// <summary>
        /// Used by implementations of hosted instances to load the assembly with the <see cref="IHostedInstanceInitializer"/> to start the instance.
        /// </summary>
        public string HostedInstanceInitializerTypeFullName { get; set; } = string.Empty;

        /// <summary>
        /// Used by implementations of hosted instances to load the assembly with the <see cref="IHostedInstanceInitializer"/> to start the instance.
        /// </summary>
        public string HostedInstanceInitializerAssemblyFullName { get; set; } = string.Empty;

        public static HostedInstanceData ParseFromCommandLineArguments(string[] args)
        {
            var hostedInstanceData = new HostedInstanceData
            {
                RunId = GetCommandLineArgumentValue<Guid>(args, RunIdArg),
                InstanceId = GetCommandLineArgumentValue<string>(args, InstanceIdArg)!,
                AssemblySourceName = GetCommandLineArgumentValue<string>(args, AssemblySourceNameArg)!,
                MainPath = GetCommandLineArgumentValue<string>(args, MainPathArg)!,
                ScenarioSuiteTypeFullName = GetCommandLineArgumentValue<string>(args, ScenarioSuiteTypeFullNameArg)!,
                HostedInstanceInitializerAssemblyFullName = GetCommandLineArgumentValue<string>(args, HostedInstanceInitializerAssemblyFullNameArg)!,
                HostedInstanceInitializerTypeFullName = GetCommandLineArgumentValue<string>(args, HostedInstanceInitializerTypeFullNameArg)!
            };

            return hostedInstanceData;
        }

        public static HostedInstanceData ParseFromEnvironmentVariables(string[] env)
        {
            var hostedInstanceData = new HostedInstanceData
            {
                RunId = GetEnvironmentVariableValue<Guid>(env, RunIdEnv),
                InstanceId = GetEnvironmentVariableValue<string>(env, InstanceIdEnv)!,
                AssemblySourceName = GetEnvironmentVariableValue<string>(env, AssemblySourceNameEnv)!,
                MainPath = GetEnvironmentVariableValue<string>(env, MainPathArg)!,
                ScenarioSuiteTypeFullName = GetEnvironmentVariableValue<string>(env, ScenarioSuiteTypeFullNameEnv)!,
                HostedInstanceInitializerAssemblyFullName = GetEnvironmentVariableValue<string>(env, HostedInstanceInitializerAssemblyFullNameEnv)!,
                HostedInstanceInitializerTypeFullName = GetEnvironmentVariableValue<string>(env, HostedInstanceInitializerTypeFullNameEnv)!
            };

            return hostedInstanceData;
        }

        public static HostedInstanceData ParseFromEnvironmentVariables(IDictionary env)
        {
            var hostedInstanceData = new HostedInstanceData
            {
                RunId = GetEnvironmentVariableValue<Guid>(env, RunIdEnv),
                InstanceId = GetEnvironmentVariableValue<string>(env, InstanceIdEnv)!,
                AssemblySourceName = GetEnvironmentVariableValue<string>(env, AssemblySourceNameEnv)!,
                MainPath = GetEnvironmentVariableValue<string>(env, MainPathEnv)!,
                ScenarioSuiteTypeFullName = GetEnvironmentVariableValue<string>(env, ScenarioSuiteTypeFullNameEnv)!,
                HostedInstanceInitializerAssemblyFullName = GetEnvironmentVariableValue<string>(env, HostedInstanceInitializerAssemblyFullNameEnv)!,
                HostedInstanceInitializerTypeFullName = GetEnvironmentVariableValue<string>(env, HostedInstanceInitializerTypeFullNameEnv)!
            };

            return hostedInstanceData;
        }

        public static HostedInstanceData FromDictionary(Dictionary<string, string> dictionary)
        {
            return new HostedInstanceData
            {
                RunId = Guid.Parse(dictionary[RunIdArg]),
                InstanceId = dictionary[InstanceIdArg],
                MainPath = dictionary[MainPathArg],
                AssemblySourceName = dictionary[AssemblySourceNameArg],
                ScenarioSuiteTypeFullName = dictionary[ScenarioSuiteTypeFullNameArg],
                HostedInstanceInitializerTypeFullName = dictionary[HostedInstanceInitializerTypeFullNameArg],
                HostedInstanceInitializerAssemblyFullName = dictionary[HostedInstanceInitializerAssemblyFullNameArg],
            };
        }

        /// <summary>
        /// Converts the hosted instance data to a list of command line arguments to be passed when starting from the command line a hosted instance.
        /// </summary>
        /// <returns></returns>
        public List<string> ToCommandLineArguments()
        {
            var args = new List<string>
            {
                $"--{RunIdArg}=\"{RunId}\"",
                $"--{InstanceIdArg}=\"{InstanceId}\"",
                $"--{MainPathArg}=\"{MainPath}\"",
                $"--{AssemblySourceNameArg}=\"{AssemblySourceName}\"",
                $"--{ScenarioSuiteTypeFullNameArg}=\"{ScenarioSuiteTypeFullName}\"",
                $"--{HostedInstanceInitializerTypeFullNameArg}=\"{HostedInstanceInitializerTypeFullName}\"",
                $"--{HostedInstanceInitializerAssemblyFullNameArg}=\"{HostedInstanceInitializerAssemblyFullName}\""
            };

            return args;
        }

        public Dictionary<string, string> ToDictionary()
        {
            return new Dictionary<string, string>
            {
                { RunIdArg, RunId.ToString() },
                { InstanceIdArg, InstanceId },
                { MainPathArg, MainPath },
                { AssemblySourceNameArg, AssemblySourceName},
                { ScenarioSuiteTypeFullNameArg, ScenarioSuiteTypeFullName },
                { HostedInstanceInitializerTypeFullNameArg, HostedInstanceInitializerTypeFullName},
                { HostedInstanceInitializerAssemblyFullNameArg, HostedInstanceInitializerAssemblyFullName}
            };
        }


        /// <summary>
        /// Converts the hosted instance data to a list of environment variables to be used when starting a hosted instance.
        /// </summary>
        /// <returns></returns>
        public List<string> ToEnvironmentVariables()
        {
            var env = new List<string>
            {
                $"{RunIdEnv}=\"{RunId}\"",
                $"{InstanceIdEnv}=\"{InstanceId}\"",
                $"{MainPathEnv}=\"{MainPath}\"",
                $"{AssemblySourceNameEnv}=\"{AssemblySourceName}\"",
                $"{ScenarioSuiteTypeFullNameEnv}=\"{ScenarioSuiteTypeFullName}\"",
                $"{HostedInstanceInitializerTypeFullNameEnv}=\"{HostedInstanceInitializerTypeFullName}\"",
                $"{HostedInstanceInitializerAssemblyFullNameEnv}=\"{HostedInstanceInitializerAssemblyFullName}\""
            };

            return env;
        }

        private static TValue? GetCommandLineArgumentValue<TValue>(string[] args, string name)
        {
            var argument = args.FirstOrDefault(arg => arg.StartsWith($"--{name}="));
            if (argument == null)
            {
                return default;
            }
            var argumentValue = argument.Substring(argument.IndexOf('=') + 1).Trim().Trim('\"');
            return (TValue?)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFromInvariantString(argumentValue);
        }

        private static TValue? GetEnvironmentVariableValue<TValue>(string[] env, string name)
        {
            var argument = env.FirstOrDefault(arg => arg.StartsWith($"{name}="));
            if (argument == null)
            {
                return default;
            }
            var argumentValue = argument.Substring(argument.IndexOf('=') + 1).Trim().Trim('\"');
            return (TValue?)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFromInvariantString(argumentValue);
        }

        private static TValue? GetEnvironmentVariableValue<TValue>(IDictionary env, string name)
        {
            var argumentValue = env[name]?.ToString();
            if (argumentValue == null)
            {
                return default;
            }
            return (TValue?)TypeDescriptor.GetConverter(typeof(TValue)).ConvertFromInvariantString(argumentValue);
        }
    }
}
