namespace SandboxTest.Engine.ApplicationRunner
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var applicationInstanceRunner = new ApplicationInstanceRunner();
            await applicationInstanceRunner.InitializeAsync(args);
            return await applicationInstanceRunner.RunAsync();
        }
    }
}
