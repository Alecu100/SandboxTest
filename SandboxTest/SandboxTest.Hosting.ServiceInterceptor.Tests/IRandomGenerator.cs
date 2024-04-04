namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    public interface IRandomGenerator
    {
        decimal GetRandomDecimal();

        double GetRandomDouble();

        Guid GetRandomGuid();

        int GetRandomInt();

        long GetRandomLong();

        short GetRandomShort();
    }
}
