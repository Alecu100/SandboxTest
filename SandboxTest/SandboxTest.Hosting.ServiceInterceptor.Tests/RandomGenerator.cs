namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    public class RandomGenerator : IRandomGenerator
    {
        private readonly Random _random = new Random();

        public Guid GetRandomGuid()
        {
            return Guid.NewGuid();
        }

        public int GetRandomInt()
        {
            return _random.Next();
        }

        public double GetRandomDouble() 
        {
            return _random.NextDouble();
        }

        public long GetRandomLong()
        {
            return _random.NextInt64();
        }

        public decimal GetRandomDecimal()
        {
            byte scale = (byte)_random.Next(29);
            bool sign = _random.Next(2) == 1;
            return new decimal(_random.Next(), _random.Next(), _random.Next(), sign, scale);
        }

        public short GetRandomShort()
        {
            return (short)_random.Next(short.MinValue, short.MaxValue + 1);
        }
    }
}
