using Newtonsoft.Json;

namespace SandboxTest.Engine.Utils
{
    /// <summary>
    /// Utility methods for pipe interactions.
    /// </summary>
    public static class PipeUtils
    {
        /// <summary>
        /// Gets the name of a pipe used to communicate with child application hosts to send commands like executing a step.
        /// </summary>
        /// <param name="runId"></param>
        /// <param name="applicationInstance"></param>
        /// <returns></returns>
        public static string GetChildApplicationInstanceHostPipeName(Guid runId, IApplicationInstance applicationInstance)
        {
            return $"application-instance-id-{applicationInstance.Id}-run-id-{runId}";
        }

        /// <summary>
        /// The default settings for serializing json objects in the json pipes.
        /// </summary>
        public static readonly JsonSerializerSettings PipeJsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}
