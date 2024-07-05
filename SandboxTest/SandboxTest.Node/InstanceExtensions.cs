using SandboxTest.Instance;

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
        public static IInstance UseNodeRunner(this IInstance instance, string host = "localhost", int port = 80, bool useSsl = false)
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
            nodeRuner.OnConfigureNode(NodeRunnerParameters.ViteParseReadyFunc, NodeRunnerParameters.ViteParseErrorFunc, sourcePath, NodeRunnerParameters.ViteNpmRunCommand);

            return instance;
        }
    }
}
