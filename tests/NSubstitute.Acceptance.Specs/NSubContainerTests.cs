using System;
using NSubstitute.Core.DependencyInjection;
using NUnit.Framework;

namespace NSubstitute.Acceptance.Specs
{
    public class NSubContainerTests
    {
        [Test]
        public void ShouldActivateRegisteredType()
        {
            var sut = new NSubContainer().Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            var result = sut.Resolve<ITestInterface>();

            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ShouldThrowIfTryToRegisterTypeWithoutPublicCtors()
        {
            var sut = new NSubContainer();

            var ex = Assert.Throws<ArgumentException>(() => sut.Register<ITestInterface, TestImplNoPublicCtors>(NSubLifetime.Transient));
            Assert.That(ex.Message, Contains.Substring("single public constructor"));
        }

        [Test]
        public void ShouldThrowIfTryToRegisterTypeWithMultipleCtors()
        {
            var sut = new NSubContainer();

            var ex = Assert.Throws<ArgumentException>(() => sut.Register<ITestInterface, TestImplMultipleCtors>(NSubLifetime.Transient));
            Assert.That(ex.Message, Contains.Substring("single public constructor"));
        }

        [Test]
        public void ShouldAllowToRegisterTypeWithMultipleCtorsUsingFactoryMethod()
        {
            var sut = new NSubContainer();

            sut.Register(_ => new TestImplMultipleCtors("42"), NSubLifetime.Transient);

            var result = sut.Resolve<TestImplMultipleCtors>();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo("42"));
        }

        [Test]
        public void ShouldResolveDependencies()
        {
            var sut = new NSubContainer();

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            var result = sut.Resolve<ClassWithDependency>();
            Assert.That(result.Dep, Is.AssignableTo<TestImplSingleCtor>());
        }

        [Test]
        public void ShouldAllowToResolveDependencyInFactoryMethod()
        {
            var sut = new NSubContainer();
            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            sut.Register<ClassWithDependency>(r => new ClassWithDependency(r.Resolve<ITestInterface>()), NSubLifetime.Transient);

            var result = sut.Resolve<ClassWithDependency>();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void ShouldReturnNewInstanceForEachRequestForTransientLifetime()
        {
            var sut = new NSubContainer();

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            var result1 = sut.Resolve<ITestInterface>();
            var result2 = sut.Resolve<ITestInterface>();
            Assert.That(result2, Is.Not.SameAs(result1));
        }

        [Test]
        public void ShouldReturnNewInstanceForSameRequestForTransientLifetime()
        {
            var sut = new NSubContainer();
            sut.Register<ClassWithMultipleDependencies, ClassWithMultipleDependencies>(NSubLifetime.Transient);
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            var result = sut.Resolve<ClassWithMultipleDependencies>();
            Assert.That(result.TestInterfaceDep, Is.Not.SameAs(result.ClassWithDependencyDep.Dep));
        }

        [Test]
        public void ShouldReturnSameInstanceForNewRequestForSingletonLifetime()
        {
            var sut = new NSubContainer();

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Singleton);

            var result1 = sut.Resolve<ITestInterface>();
            var result2 = sut.Resolve<ITestInterface>();
            Assert.That(result2, Is.SameAs(result1));
        }

        [Test]
        public void ShouldReturnSameInstanceForSameRequestForSingletonLifetime()
        {
            var sut = new NSubContainer();
            sut.Register<ClassWithMultipleDependencies, ClassWithMultipleDependencies>(NSubLifetime.Transient);
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Singleton);

            var result = sut.Resolve<ClassWithMultipleDependencies>();
            Assert.That(result.TestInterfaceDep, Is.SameAs(result.ClassWithDependencyDep.Dep));
        }

        [Test]
        public void ShouldReturnNewInstanceForNewRequestForPerScopeLifetime()
        {
            var sut = new NSubContainer();

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.PerScope);

            var result1 = sut.Resolve<ITestInterface>();
            var result2 = sut.Resolve<ITestInterface>();
            Assert.That(result2, Is.Not.SameAs(result1));
        }

        [Test]
        public void ShouldReturnSameInstanceForSameRequestForPerScopeLifetime()
        {
            var sut = new NSubContainer();
            sut.Register<ClassWithMultipleDependencies, ClassWithMultipleDependencies>(NSubLifetime.Transient);
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.PerScope);

            var result = sut.Resolve<ClassWithMultipleDependencies>();
            Assert.That(result.TestInterfaceDep, Is.SameAs(result.ClassWithDependencyDep.Dep));
        }

        [Test]
        public void ShouldReturnSameInstanceWhenResolvingDependencyInFactoryMethodForPerScopeLifetime()
        {
            var sut = new NSubContainer();
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.PerScope);
            sut.Register<ClassWithMultipleDependencies>(
                r => new ClassWithMultipleDependencies(r.Resolve<ITestInterface>(), r.Resolve<ClassWithDependency>()),
                NSubLifetime.Transient);

            var result = sut.Resolve<ClassWithMultipleDependencies>();
            Assert.That(result.TestInterfaceDep, Is.SameAs(result.ClassWithDependencyDep.Dep));
        }

        [Test]
        public void ShouldUseNewRegistrationOnRepeatedRegister()
        {
            var sut = new NSubContainer();

            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);
            sut.Register<ITestInterface, TestImplSingleCtor2>(NSubLifetime.Transient);

            var result = sut.Resolve<ITestInterface>();
            Assert.That(result, Is.AssignableTo<TestImplSingleCtor2>());
        }

        [Test]
        public void ShouldCreateNewContainerInstanceOnCustomize()
        {
            var sut = new NSubContainer();

            var sutFork = sut.Customize();

            Assert.That(sutFork, Is.Not.SameAs(sut));
        }

        [Test]
        public void ShouldNotModifyOriginalContainerOnCustomize()
        {
            var sut = new NSubContainer();
            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            var sutFork = sut.Customize().Register<ITestInterface, TestImplSingleCtor2>(NSubLifetime.Transient);

            var sutResult = sut.Resolve<ITestInterface>();
            var sutForkResult = sutFork.Resolve<ITestInterface>();
            Assert.That(sutResult, Is.AssignableTo<TestImplSingleCtor>());
            Assert.That(sutForkResult, Is.AssignableTo<TestImplSingleCtor2>());
        }

        [Test]
        public void ShouldReturnFromParentContainerIfNoForkCustomizations()
        {
            var sut = new NSubContainer();
            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.Transient);

            var sutFork = sut.Customize().Customize().Customize();

            var result = sutFork.Resolve<ITestInterface>();
            Assert.That(result, Is.AssignableTo<TestImplSingleCtor>());
        }

        [Test]
        public void ShouldFailWithMeaningfulExceptionIfUnableToResolveType()
        {
            var sut = new NSubContainer();

            var ex = Assert.Throws<InvalidOperationException>(() => sut.Resolve<ITestInterface>());
            Assert.That(ex.Message, Contains.Substring("not registered"));
            Assert.That(ex.Message, Contains.Substring(typeof(ITestInterface).FullName));
        }

        [Test]
        public void ShouldReturnSameValueWithinSameExplicitScope()
        {
            var sut = new NSubContainer();
            sut.Register<ITestInterface, TestImplSingleCtor>(NSubLifetime.PerScope);
            sut.Register<ClassWithDependency, ClassWithDependency>(NSubLifetime.Transient);

            var scope = sut.CreateScope();
            var result1 = scope.Resolve<ClassWithDependency>();
            var result2 = scope.Resolve<ClassWithDependency>();

            Assert.That(result1, Is.Not.SameAs(result2));
            Assert.That(result1.Dep, Is.SameAs(result2.Dep));
        }

        public interface ITestInterface
        {
        }

        public class TestImplSingleCtor : ITestInterface
        {
        }

        public class TestImplSingleCtor2 : ITestInterface
        {
        }

        public class TestImplMultipleCtors : ITestInterface
        {
            public string Value { get; }

            public TestImplMultipleCtors()
            {
            }

            public TestImplMultipleCtors(string value)
            {
                Value = value;
            }
        }

        public class TestImplNoPublicCtors : ITestInterface
        {
            private TestImplNoPublicCtors()
            {
            }
        }

        public class ClassWithDependency
        {
            public ITestInterface Dep { get; }

            public ClassWithDependency(ITestInterface dep)
            {
                Dep = dep;
            }
        }

        public class ClassWithMultipleDependencies
        {
            public ITestInterface TestInterfaceDep { get; }
            public ClassWithDependency ClassWithDependencyDep { get; }

            public ClassWithMultipleDependencies(ITestInterface testInterfaceDep, ClassWithDependency classWithDependencyDep)
            {
                TestInterfaceDep = testInterfaceDep;
                ClassWithDependencyDep = classWithDependencyDep;
            }
        }
    }
}