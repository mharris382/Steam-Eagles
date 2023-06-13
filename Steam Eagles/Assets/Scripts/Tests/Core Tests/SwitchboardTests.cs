using System;
using System.Collections.Generic;
using CoreLib;
using NUnit.Framework;

namespace Tests.Core_Tests
{
    public class SmartSwitchboardTests
    {
        private Switchboard<Foo> _switchboard;
        private List<Foo> _foos;

        [SetUp]
        public void SetUp()
        {
            Foo.count = 0;
            _switchboard = new EvenOnlyBoard();
            _foos = new();
        }

        [TearDown]
        public void TearDown()
        {
            _switchboard.Dispose();
        }

        class EvenOnlyBoard : SmartSwitchboard<Foo>
        {
            public bool invertCondition;
            protected override bool IsValid(int index, Foo value)
            {
                return value.ID % 2 == (invertCondition? 1 : 0);
            }
        }
        private Foo AddFooToSwitchboard()
        {
            var foo = new Foo();
            _foos.Add(foo);
            _switchboard.Add(foo);
            return foo;
        }

        private Foo[] AddFooToSwitchboard(int amount)
        {
            Foo[] res = new Foo[amount];
            for (int i = 0; i < amount; i++)
            {
                res[i] = AddFooToSwitchboard();
            }

            return res;
        }
        [Test]
        public void SelectsFirstOdd()
        {
            var foos = AddFooToSwitchboard(4);
            ((EvenOnlyBoard)_switchboard).invertCondition = true;
            ((ISlowTickable)_switchboard).SlowTick(0);
            Assert.AreEqual(foos[1], _switchboard.Current);
        }
        [Test]
        public void SelectsFirstEven()
        {
            var foos = AddFooToSwitchboard(4);
            Assert.AreEqual(foos[0], _switchboard.Current);
             Assert.IsTrue(_switchboard.Next());
            Assert.AreEqual(foos[2], _switchboard.Current);
             Assert.IsTrue(_switchboard.Next());
            Assert.AreEqual(foos[0], _switchboard.Current);
        }
        [Test]
        public void NextSelectsEven()
        {
            var foos = AddFooToSwitchboard(4);
            Assert.AreEqual(foos[0], _switchboard.Current);
            Assert.IsTrue(_switchboard.Next());
            Assert.AreEqual(foos[2], _switchboard.Current);
            Assert.IsTrue(_switchboard.Next());
            Assert.AreEqual(foos[0], _switchboard.Current);
        }
        [Test]
        public void PrevSelectsEven()
        {
            var foos = AddFooToSwitchboard(4);
            Assert.AreEqual(foos[0], _switchboard.Current);
            Assert.IsTrue(_switchboard.Prev());
            Assert.AreEqual(foos[2], _switchboard.Current);
            Assert.IsTrue(_switchboard.Prev());
            Assert.AreEqual(foos[0], _switchboard.Current);
        }
        [Test]
        public void ReSelectsFirstEven()
        {
            var foos = AddFooToSwitchboard(6);
            Assert.AreEqual(foos[0], _switchboard.Current);
            Assert.IsTrue(_switchboard.Remove(foos[0]));
            Assert.IsTrue(_switchboard.Prev());
            Assert.AreEqual(foos[2], _switchboard.Current);
            Assert.IsTrue(_switchboard.Prev());
            Assert.AreEqual(foos[4], _switchboard.Current);
        }
        [Test]
        public void ReSelectsIfConditionChanges()
        {
            var foos = AddFooToSwitchboard(6);
            Assert.AreEqual(foos[0], _switchboard.Current);
            ((EvenOnlyBoard)_switchboard).invertCondition = true;
            ((ISlowTickable)_switchboard).SlowTick(0);
            Assert.AreNotEqual(foos[0], _switchboard.Current);
            Assert.AreEqual(foos[1], _switchboard.Current);
        }
}
    public class SwitchboardTests
    {
        private Switchboard<Foo> _switchboard;
        private FooBoard _fooBoard;
        private List<Foo> _foos;
        [SetUp]
        public void SetUp()
        {
            Foo.count = 0;
            _fooBoard = new FooBoard();
            _switchboard = _fooBoard;
            _foos = new();
        }

        [TearDown]
        public void TearDown()
        {
            _switchboard.Dispose();
        }
        private Foo AddFooToSwitchboard()
        {
            var foo = new Foo();
            _foos.Add(foo);
            _switchboard.Add(foo);
            return foo;
        }

        private Foo[] AddFooToSwitchboard(int amount)
        {
            Foo[] res = new Foo[amount];
            for (int i = 0; i < amount; i++)
            {
                res[i] = AddFooToSwitchboard();
            }

            return res;
        }

        [Test]
        public void SetsToNullOnRemoval()
        {
            var foo = AddFooToSwitchboard();
            Assert.IsTrue(_switchboard.Current.Equals(foo));
            _switchboard.Remove(foo);
            Assert.IsNull(_switchboard.Current);
        }

        [Test]
        public void SwitchboardAutoSelectsFirstItem()
        {
            Assert.IsTrue(_switchboard.Current == null);
            var foo = AddFooToSwitchboard();
            Assert.IsTrue(_switchboard.Current == foo);
        }
        
        [Test]
        public void SwitchboardDoesNotAutoSelectSecond()
        {
            Assert.IsTrue(_switchboard.Current == null);
            var foo1 = AddFooToSwitchboard();
            Assert.IsTrue(_switchboard.Current == foo1);
            var foo2 = AddFooToSwitchboard();
            Assert.IsTrue(_switchboard.Current == foo1);
        }

        [Test]
        public void CannotAddSameItemTwice()
        {
            var foo = AddFooToSwitchboard();
            Assert.IsFalse(_switchboard.Add(foo));
        }
        [Test]
        public void SwitchboardSelectsPrevious()
        {
            Foo[] results = AddFooToSwitchboard(3);
            Assert.AreEqual(results[0], _switchboard.Current);
            _switchboard.Prev();
            Assert.AreEqual(results[2], _switchboard.Current);
            _switchboard.Prev();
            Assert.AreEqual(results[1], _switchboard.Current);
            _switchboard.Prev();
            Assert.AreEqual(results[0], _switchboard.Current);
        }
        [Test]
        public void SwitchboardSelectsNext()
        {
            Foo[] results = AddFooToSwitchboard(3);
            Assert.AreEqual(results[0], _switchboard.Current);
            _switchboard.Next();
            Assert.AreEqual(results[1], _switchboard.Current);
            _switchboard.Next();
            Assert.AreEqual(results[2], _switchboard.Current);
            _switchboard.Next();
            Assert.AreEqual(results[0], _switchboard.Current);
        }

        [Test]
        public void SwitchboardSelectsNextWhenSelectionIsRemoved()
        {
            var foos = AddFooToSwitchboard(3);
            Assert.AreEqual(foos[0], _switchboard.Current);
            _switchboard.Remove(foos[0]);
            Assert.AreNotEqual(foos[0], _switchboard.Current);
            Assert.AreNotEqual(foos[0], null);
            _switchboard.Next();
            Assert.AreEqual(foos[1], _switchboard.Current);
        }

        [Test]
        public void SwitchboardCallsFunctions()
        {
            Assert.AreEqual(null, _fooBoard.LastActivatedValue);
            Assert.AreEqual(null, _fooBoard.LastDeactivatedValue);
            var foos = AddFooToSwitchboard(3);
            Assert.AreEqual(foos[0] , _fooBoard.LastActivatedValue);
            Assert.AreEqual(null, _fooBoard.LastDeactivatedValue);
            _switchboard.Next();
            Assert.AreEqual(foos[0] , _fooBoard.LastDeactivatedValue);
            Assert.AreEqual(foos[1] , _fooBoard.LastActivatedValue);
        }

        [Test]
        public void SwitchboardRemovesItems()
        {
            var foos = AddFooToSwitchboard(3);
            Assert.AreEqual(3, _switchboard.Count);
            Assert.IsTrue(_switchboard.Remove(foos[0]));
            Assert.AreEqual(2, _switchboard.Count);
            Assert.IsTrue(_switchboard.Remove(foos[1]));
            Assert.AreEqual(1, _switchboard.Count);
            Assert.IsTrue(_switchboard.Remove(foos[2]));
            Assert.AreEqual(0, _switchboard.Count);
        }

        class FooBoard : Switchboard<Foo>
        {
            public Foo LastActivatedValue { get; private set;}
            public Foo LastDeactivatedValue { get; private set;}
            protected override void ValueActivated(Foo value) => LastActivatedValue = value;
            protected override void ValueDeactivated(Foo value) => LastDeactivatedValue = value;
        }
    }
       internal  class Foo : IEquatable<Foo>
        {
            public static int count;

            public int ID { get;  }
            public Foo() => ID = count++;
            public Foo(int ID) => this.ID = ID;

            public bool Equals(Foo other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return ID == other.ID;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Foo)obj);
            }

            public override int GetHashCode()
            {
                return ID;
            }

            public override string ToString()
            {
                return $"Foo{ID}";
            }
        }
        
    
    
    
}