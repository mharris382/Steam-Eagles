using System;
using CoreLib.Extensions;
using NUnit.Framework;
using UniRx;
using UnityEngine;

namespace Tests.Core_Tests
{
    public class UniRxExtensionTests
    {

        class DistinctWhereTest
        {
            public int value;
            public bool suppress = false;
            public string name = "";
        }

        private CompositeDisposable cd;
        Subject<DistinctWhereTest> stream = new();
        Subject<DistinctWhereTest> streamSuppressed = new();
        private DistinctWhereTest[] _whereTests;
        int[] recievedCount = new int[3];
        int[] suppressedCount = new int[3];
        [SetUp]
        public void SetUp()
        {
            cd = new CompositeDisposable();
            stream = new();
            streamSuppressed = new();
            stream.AddTo(cd);
            streamSuppressed.AddTo(cd);
            

        }

        [TearDown]
        public void TearDown()
        {
            cd.Dispose();
        }
        

        void SetupTestValues(int count)
        {
            DistinctWhereTest[] GetTestValues(int v)
            {
                DistinctWhereTest[] values = new DistinctWhereTest[v];
                for (int i = 0; i < v; i++)
                {
                    values[i] = new DistinctWhereTest()
                    {
                        value = i,
                        name = i.ToString(),
                    };
                }
                return values;
            }
            _whereTests = GetTestValues(count);
            recievedCount = new int[count];
            suppressedCount = new int[count];
            void OnTestValueRecieved(DistinctWhereTest t) => recievedCount[t.value]++;
            void OnSuppressedValueRecieved(DistinctWhereTest t) => suppressedCount[t.value]++;
            stream.Subscribe(OnTestValueRecieved).AddTo(cd);
            streamSuppressed.Subscribe(OnSuppressedValueRecieved).AddTo(cd);
        }
        [Test]
        public void TestDistinctWhere()
        {
            SetupTestValues(5);
            
            Func<DistinctWhereTest, DistinctWhereTest, bool> comparer = (t1, t2) => t1.value == t2.value;
            stream.DistinctWhere(comparer).Subscribe(t => Debug.Log($"Recieved {t.value}")).AddTo(cd);
            EmitTestValues(_whereTests);
            for (int i = 0; i < _whereTests.Length; i++)
            {
                Assert.AreEqual(1, recievedCount[i]);
                Assert.AreEqual(1, suppressedCount[i]);
            }
            EmitTestValues(_whereTests);
            for (int i = 0; i < _whereTests.Length; i++)
            {
                Assert.AreEqual(2, recievedCount[i]);
                Assert.AreEqual(2, suppressedCount[i]);
            }
            EmitTestValue(_whereTests[^1]);
            Assert.AreEqual(3, recievedCount[^1]);
            Assert.AreEqual(2, suppressedCount[^1]);
            EmitTestValues(_whereTests);
            for (int i = 0; i < _whereTests.Length; i++)
            {
                Assert.AreEqual(4, recievedCount[i]);
                Assert.AreEqual(3, suppressedCount[i]);
            }
        }
        void EmitTestValue(DistinctWhereTest t)
        {
            stream.OnNext(t);
        }

        void EmitTestValues(DistinctWhereTest[] values)
        {
            foreach (var distinctWhere in values)
            {
                EmitTestValue(distinctWhere);
            }
        }
    }
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