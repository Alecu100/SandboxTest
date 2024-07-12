namespace SandboxTest.Sample.Application5.Server
{
    public static class WebApplicationExtensions
    {
        public static WebApplicationBuilder ConfigureWebApplicationBuilder(this WebApplicationBuilder builder)
        {

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            return builder;
        }

        public static WebApplication ConfigureWebApplication(this WebApplication webApplication)
        {
            webApplication.UseDefaultFiles();
            webApplication.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (webApplication.Environment.IsDevelopment())
            {
                webApplication.UseSwagger();
                webApplication.UseSwaggerUI();
            }

            webApplication.UseHttpsRedirection();

            webApplication.UseAuthorization();


            webApplication.MapControllers();

            webApplication.MapFallbackToFile("/index.html");
            return webApplication;
        }
    }
}
