namespace SandboxTest.Instance
{
    public abstract class ControllerBase : IController
    {
        protected string? _name;

        protected ControllerBase(string? name) 
        {
            _name = name;
        }

        /// <inheritdoc/>
        public string? Name { get => _name; }
    }
}
