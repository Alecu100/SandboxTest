namespace SandboxTest.Node
{
    public static class NodeRunnerParameters
    {
        public static string ViteNpmRunCommand = "run dev";

        public static bool ViteParseReadyFunc(string consoleMessage)
        {
            var viteIndex = consoleMessage.IndexOf("VITE");
            if (viteIndex < 0)
            {
                return false;
            }
            var readyInIndex = consoleMessage.IndexOf("ready in", viteIndex);
            if (readyInIndex < 0) 
            {
                return false;
            }
            return true;
        }

        public static bool ViteParseErrorFunc(string consoleMessage)
        {
            return consoleMessage.Contains("error", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
