using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Tests.Core_Tests
{
    public class ReactiveCollectionTests
    {
        private ReactiveCollection<string> _collection = new ReactiveCollection<string>();
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        [SetUp]
        public void SetUp()
        {
            if(_compositeDisposable != null)_compositeDisposable.Dispose();
            _compositeDisposable = new CompositeDisposable();   
            _collection = new ReactiveCollection<string>();
        }

        [TearDown]
        public void TearDown()
        {
            _compositeDisposable?.Dispose();
            _compositeDisposable = null;
            _collection?.Dispose();
            _collection = null;
        }
        [Test]
        public void TestObserveAdd()
        {
            string[] toAdd = new string[]
            {
                "test",
                "2",
                "run berry run",
            };
            int recieved = 0;
            _collection.ObserveAdd().Subscribe(t =>
            {
                Assert.AreEqual(toAdd[recieved],t.Value);
                recieved++;
            });
            for (int i = 0; i < toAdd.Length; i++)
            {
                Assert.AreEqual(i, recieved);
                _collection.Add(toAdd[i]);
            }
        }
        
        [Test]
        public void TestObserveRemoved()
        {
            string[] toAdd = new string[]
            {
                "test",
                "2",
                "run berry run",
            };
            TestObserveAdd();
            int recieved = 0;
            _collection.ObserveRemove().Subscribe(t =>
            {
                Assert.AreEqual(toAdd[recieved],t.Value);
                recieved++;
            });
            for (int i = 0; i < toAdd.Length; i++)
            {
                Assert.AreEqual(i, recieved);
                _collection.Remove(toAdd[i]);
            }
        }

        [Test]
        public void TestCountChanged()
        {
            _collection.Clear();
            TestObserveAdd();
            int recieved = 0;
            _collection.ObserveCountChanged(true).Subscribe(t =>
            {
                Assert.AreEqual(3-recieved,t);
                recieved++;
            });
            string[] toAdd = new string[]
            {
                "test",
                "2",
                "run berry run",
            };
            int cnt = 0;
            foreach (var s in toAdd)
            {
                Assert.IsTrue(_collection.Contains(s));
                Assert.AreEqual(cnt, _collection.IndexOf(s));
                cnt++;
            }
            Assert.AreEqual(1, recieved);
            TestObserveRemoved();
            Assert.AreEqual(4, recieved);
        }
    }
}