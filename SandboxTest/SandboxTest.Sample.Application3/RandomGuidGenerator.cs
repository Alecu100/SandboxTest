namespace SandboxTest.Sample.Application3
{
    public class RandomGuidGenerator : IRandomGuidGenerator
    {
        public Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
