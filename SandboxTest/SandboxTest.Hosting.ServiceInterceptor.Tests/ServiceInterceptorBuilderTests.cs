using FluentAssertions;
using SandboxTest.Application;
using SandboxTest.Dummy;
using SandboxTest.Instance;
using SandboxTest.Scenario;

namespace SandboxTest.Hosting.ServiceInterceptor.Tests
{
    [ScenarioSuite]
    public class ServiceInterceptorBuilderTests
    {
        public readonly IInstance _dummyApplicationInstance = ApplicationInstance.CreateEmptyInstance("DummyApplication")
            .UseDummyRunner()
            .AddDummyController();

        [Scenario]
        public void Test_Simple_GenericInterface_WithMethodAndProperty()
        {
            _dummyApplicationInstance.AddStep().UseController<DummyController>((controller, context) =>
            {
                var serviceInterceptorController = new ServiceInterceptorController();
                var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(typeof(ITestInterface<>), typeof(TestInterfaceClass<>), serviceInterceptorController);
                var serviceInterceptorListType = serviceInterceptorTypeBuilder.Build();
                var concreteType = serviceInterceptorListType.MakeGenericType(new Type[] { typeof(TestClassDerived) });
                var instance = Activator.CreateInstance(concreteType, new object[] { serviceInterceptorController }) as ITestInterface<TestClassDerived>;
                instance!.Add(new TestClassDerived());
                var testClass = instance.Get(0);
                var count = instance.Count;
                testClass.Should().NotBeNull();
                count.Should().Be(1);
                return Task.CompletedTask;
            });
        }

        [Scenario]
        public void Test_ListClass_With_ListInterface()
        {
            _dummyApplicationInstance.AddStep().UseController<DummyController>((controller, context) =>
            {
                var serviceInterceptorController = new ServiceInterceptorController();
                var serviceInterceptorTypeBuilder2 = new ServiceInterceptorTypeBuilder(typeof(IList<>), typeof(List<>), serviceInterceptorController);
                var listTestClassType = serviceInterceptorTypeBuilder2.Build();
                var listTestClassTypeConcrete = listTestClassType.MakeGenericType(new Type[] { typeof(TestClassDerived) });
                var concreteListTestClass2 = Activator.CreateInstance(listTestClassTypeConcrete, new object[] { serviceInterceptorController, new List<TestClassDerived> { new TestClassDerived() } }) as IList<TestClassDerived>;
                concreteListTestClass2!.Add(new TestClassDerived());
                var count2 = concreteListTestClass2.Count;
                count2.Should().Be(2);
                return Task.CompletedTask;
            });
        }

        [Scenario]
        public void Test_InterfaceWithGenericMethod_With_ClassWithGenericMethod()
        {
            _dummyApplicationInstance.AddStep().UseController<DummyController>((controller, context) =>
            {
                var serviceInterceptorController = new ServiceInterceptorController();
                var serviceInterceptorTypeBuilder3 = new ServiceInterceptorTypeBuilder(typeof(IGenericMethodInterface), typeof(GenericMethodInterfaceClass), serviceInterceptorController);
                var concreteGenericMethodInterface = serviceInterceptorTypeBuilder3.Build();
                var concreteGenericMethodInterfaceClass = Activator.CreateInstance(concreteGenericMethodInterface, serviceInterceptorController) as IGenericMethodInterface;
                concreteGenericMethodInterfaceClass!.PrintToConsoleGeneric(new TestClass { Name = "Test Generic Method" });
                return Task.CompletedTask;
            });
        }

        [Scenario]
        public void Test_InterfaceWithValueTypeReturnMethods_With_ClassWithValueTypeReturnMethods()
        {
            _dummyApplicationInstance.AddStep().UseController<DummyController>((controller, context) =>
            {
                var serviceInterceptorController = new ServiceInterceptorController();
                var serviceInterceptorTypeBuilder4 = new ServiceInterceptorTypeBuilder(typeof(IRandomGenerator), typeof(RandomGenerator), serviceInterceptorController);
                var randomGeneratorInterceptorType = serviceInterceptorTypeBuilder4.Build();
                var randomGenerator = Activator.CreateInstance(randomGeneratorInterceptorType, serviceInterceptorController) as IRandomGenerator;
                var randomShort = randomGenerator!.GetRandomShort();
                var randomInt = randomGenerator!.GetRandomInt();
                var randomLong = randomGenerator!.GetRandomLong();
                var randomDouble = randomGenerator!.GetRandomDouble();
                var randomDecimal = randomGenerator!.GetRandomDecimal();
                var randomGuid = randomGenerator!.GetRandomGuid();
                return Task.CompletedTask;
            });
        }
    }
}
