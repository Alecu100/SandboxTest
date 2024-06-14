namespace SandboxTest.Engine.ApplicationContainer
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var applicationInstanceRunner = new ApplicationInstanceContainer();
            await applicationInstanceRunner.InitializeAsync(args);
            return await applicationInstanceRunner.RunAsync();
        }
    }
}
