namespace SandboxTest.Sample.Application1
{
    public class ConsoleService : IConsoleService
    {
        public void WriteToConsole(string text)
        {
            Console.WriteLine(text);
        }
    }
}
