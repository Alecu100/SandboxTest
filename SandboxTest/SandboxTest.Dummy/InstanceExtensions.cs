using SandboxTest.Instance;

namespace SandboxTest.Dummy
{
    public static class InstanceExtensions
    {
        /// <summary>
        /// Assigns a <see cref="DummyRunner"/> as the runner to the instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static IInstance UseDummyRunner(this IInstance instance)
        {
            instance.UseRunner(new DummyRunner());
            return instance;
        }

        /// <summary>
        /// Adds an application controller of type <see cref="DummyController"/> to the given instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IInstance AddDummyController(this IInstance instance, string? name = default)
        {
            var dummyApplicationRunner = instance.Runner as DummyRunner;
            if (dummyApplicationRunner == null)
            {
                throw new InvalidOperationException("Invalid runner configured on instance, expected DummyRunner");
            }

            var dummyApplicationController = new DummyController(name);
            instance.AddController(dummyApplicationController);
            return instance;
        }
    }
}
