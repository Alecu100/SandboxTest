namespace SandboxTest.Sample.Application1
{
    public class KeyedConsoleService : IConsoleService
    {
        public void WriteToConsole(string text)
        {
            Console.WriteLine("-----------------------------------");
            Console.WriteLine(text);
            Console.WriteLine("-----------------------------------");
        }
    }
}
