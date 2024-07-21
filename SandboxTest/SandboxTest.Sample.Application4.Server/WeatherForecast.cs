namespace SandboxTest.Sample.Application4.Server
{
    public class WeatherForecast
    {
#if NET7_0_OR_GREATER
        public DateOnly Date { get; set; }
#endif

#if NET6_0
        public DateTime Date { get; set; }
#endif

        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

        public string? Summary { get; set; }
    }
}
