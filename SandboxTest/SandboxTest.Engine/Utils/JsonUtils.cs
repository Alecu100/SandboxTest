using Newtonsoft.Json;

namespace SandboxTest.Engine.Utils
{
    /// <summary>
    /// Utility methods for pipe interactions.
    /// </summary>
    public static class JsonUtils
    {
        /// <summary>
        /// The default settings for serializing json objects in the json pipes.
        /// </summary>
        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };
    }
}
