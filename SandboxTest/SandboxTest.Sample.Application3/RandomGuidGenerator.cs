namespace SandboxTest.Sample.Application1
{
    public class RandomGuidGenerator : IRandomGuidGenerator
    {
        public Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }
    }
}
