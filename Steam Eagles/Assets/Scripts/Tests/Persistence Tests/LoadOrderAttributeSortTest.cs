using System.Collections.Generic;
using NUnit.Framework;
using SaveLoad;

namespace Tests.Persistence_Tests
{
    [TestFixture]
    public class LoadOrderAttributeSortTest
    {
        [Test]
        public void TestLoadOrderIsSortedCorrectly()
        {
            var expectedOrder = new[]
            {
                new LoadOrderAttribute(-1000),
                new LoadOrderAttribute(-100),
                new LoadOrderAttribute(-10),
                new LoadOrderAttribute(-1),
                new LoadOrderAttribute(0),
                new LoadOrderAttribute(1),
                new LoadOrderAttribute(10),
                new LoadOrderAttribute(100),
                new LoadOrderAttribute(1000),
            };
            List<LoadOrderAttribute> loadOrderAttributes = new List<LoadOrderAttribute>(expectedOrder);
            loadOrderAttributes.Sort();
            for (int i = 0; i < expectedOrder.Length; i++)
            {
                Assert.AreEqual(expectedOrder[i], loadOrderAttributes[i]);
            }
        }
    }
}