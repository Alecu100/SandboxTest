namespace SandboxTest.Sample.Application3
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
