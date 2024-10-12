namespace SandboxTest.Sample.Application6
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder = builder.ConfigureWebApplicationBuilder();

            var app = builder.Build();
            app.Use(Middleware404);
            app.ConfigureWebApplication();
            app.Run();
        }

        private static async Task Middleware404(HttpContext ctx, Func<Task> next)
        {
            await next();
            if (ctx.Response.StatusCode == 404)
            {
                int a = 0;
                a++;
            }
        }
    }
}
