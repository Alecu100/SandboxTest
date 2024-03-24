﻿using SandboxTest.Hosting.ServiceInterceptor;

namespace SandboxTest.Hosting.ServiceInterceptor.ManualDebugger
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var serviceInterceptorController = new ServiceInterceptorController();
            var serviceInterceptorTypeBuilder = new ServiceInterceptorTypeBuilder(typeof(ITestInterface<>), typeof(TestInterfaceClass<>), serviceInterceptorController);
            var serviceInterceptorListType = serviceInterceptorTypeBuilder.Build();
            var concreteType = serviceInterceptorListType.MakeGenericType(new Type[] { typeof(TestClassDerived) });
            var instance = Activator.CreateInstance(concreteType, new object[] { serviceInterceptorController }) as ITestInterface<TestClassDerived>;
            instance.Add(new TestClassDerived());
            var testClass = instance.Get(0);
            var count = instance.Count;
            var serviceInterceptorTypeBuilder2 = new ServiceInterceptorTypeBuilder(typeof(IList<>), typeof(List<>), serviceInterceptorController);
            var listTestClassType = serviceInterceptorTypeBuilder2.Build();
            var listTestClassTypeConcrete = listTestClassType.MakeGenericType(new Type[] { typeof(TestClassDerived) });
            var concreteListTestClass = Activator.CreateInstance(listTestClassTypeConcrete, serviceInterceptorController) as IList<TestClassDerived>;
            concreteListTestClass.Add(new TestClassDerived());
            var count2 = concreteListTestClass.Count;
            Console.WriteLine("Hello, World!");
        }
    }
}
