namespace SandboxTest.Sample.Application6
{
    public static class WebApplicationExtensions
    {
        public static WebApplicationBuilder ConfigureWebApplicationBuilder(this WebApplicationBuilder builder)
        {
            // Add services to the container.
            builder.Services.AddRazorPages();

            return builder;
        }

        public static WebApplication ConfigureWebApplication(this WebApplication webApplication)
        {
            // Configure the HTTP request pipeline.
            if (!webApplication.Environment.IsDevelopment())
            {
                webApplication.UseExceptionHandler("/Error");
            }
            webApplication.UseStaticFiles();

            webApplication.UseRouting();

            webApplication.UseAuthorization();

            webApplication.MapRazorPages();

            return webApplication;
        }
    }
}
