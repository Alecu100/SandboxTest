namespace SandboxTest.Hosting.Tests
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
