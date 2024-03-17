namespace SandboxTest.Sample.Application3
{
    public class ConsoleService : IConsoleService
    {
        public void WriteToConsole(string text)
        {
            Console.WriteLine(text);
        }
    }
}
