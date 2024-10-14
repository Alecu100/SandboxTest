using SandboxTest.Instance;
using SandboxTest.Instance.AttachedMethod;
using SandboxTest.Scenario;

namespace SandboxTest.Node
{
    /// <summary>
    /// Extensions methods to add and configure the node runner.
    /// </summary>
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="NodeRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="instance">The instance to add the node runner to.</param>
        /// <param name="host">The host name under which to start the node runner</param>
        /// <param name="port"></param>
        /// <param name="useSsl"></param>
        /// <returns></returns>
        public static IInstance UseNodeRunner(this IInstance instance, string host = "localhost", int port = 80, bool useSsl = true)
        {
            instance.UseRunner(new NodeRunner(host, port, useSsl));
            return instance;
        }

        /// <summary>
        /// Configures the web application in such a way to be able to run in an scenario.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance ConfigureNodeRunnerWithVite(this IInstance instance, string sourcePath)
        {
            var nodeRuner = instance.Runner as NodeRunner;
            if (nodeRuner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected node runner");
            }

            Func<IScenarioSuiteContext, Task> onConfigureBuild = (ctx) =>
            {
                nodeRuner.SourcePath = sourcePath;
                nodeRuner.ParseReadyFunc = NodeRunnerParameters.ViteParseReadyFunc;
                nodeRuner.ParseErrorFunc = NodeRunnerParameters.ViteParseErrorFunc;
                nodeRuner.NpmRunCommand = NodeRunnerParameters.ViteNpmRunCommand;
                return Task.CompletedTask;
            };
            nodeRuner.AddAttachedMethod(AttachedMethodType.RunnerToRunner, onConfigureBuild, nameof(onConfigureBuild), nameof(nodeRuner.BuildAsync), -100);

            return instance;
        }
    }
}
