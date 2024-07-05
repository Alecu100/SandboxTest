
namespace SandboxTest.Sample.Application4.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder = builder.ConfigureWebApplicationBuilder();

            var app = builder.Build();
            app.ConfigureWebApplication();
            app.Run();
        }
    }
}
