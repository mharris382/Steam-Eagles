using System.Collections;
using CoreLib;
using NUnit.Framework;
using UnityEngine.TestTools;
using UniRx;
using Zenject;

namespace Tests.Core_Tests
{
    public class RegistryTests : ZenjectUnitTestFixture
    {
        const int TEST_COUNT = 35;

        [SetUp]
        public void SetUp()
        {
            Container.Bind<RegistryBase<Foo>>().To<FooRegistry>().AsSingle().NonLazy();
        }

        [Test]
        public void PublishesOnValueAdded()
        {
            var registry = Container.Resolve<RegistryBase<Foo>>();
            Assert.NotNull(registry);
            Foo lastValue = null;
            using (registry.OnValueAdded.Subscribe(t => lastValue = t))
            {
                for (int i = 0; i < TEST_COUNT; i++)
                {
                    var next = new Foo(i);
                    registry.Register(next);
                    Assert.AreEqual(next, lastValue);
                }
            }
        }

        [Test]
        public void PublishesOnValueRemoved()
        {
            var registry = Container.Resolve<RegistryBase<Foo>>();
            Assert.NotNull(registry);
            Foo lastValue = null;
            Foo[] values = new Foo[TEST_COUNT];
            for (int i = 0; i < TEST_COUNT; i++)
            {
                values[i] = new Foo(i);
                registry.Register(values[i]);
            }
            Assert.AreEqual(TEST_COUNT, registry.ValueCount.Value);
            using (registry.OnValueRemoved.Subscribe(t => lastValue = t)) {
                for (int i = 0; i < TEST_COUNT; i++)
                {
                    var valueToRemove = values[i];
                    registry.Unregister(valueToRemove);
                    Assert.AreEqual(valueToRemove, lastValue);
                }
            }
        }
        
        [Test]
        public void PublishesCorrectCount()
        {
            var registry = Container.Resolve<RegistryBase<Foo>>();
            Assert.NotNull(registry);
            Assert.AreEqual(0, registry.ValueCount.Value);
            for (int i = 0; i < TEST_COUNT; i++)
            {
                registry.Register(new Foo(i));
                Assert.AreEqual(i+1, registry.ValueCount.Value);
            }
        }
        
        
     
        public class Foo
        {
            private readonly int _foo;

            public Foo(int foo)
            {
                _foo = foo;
            }
        }
        public class FooRegistry : RegistryBase<Foo> { }
    }
}