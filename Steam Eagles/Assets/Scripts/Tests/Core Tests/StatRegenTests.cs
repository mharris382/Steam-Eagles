using System;
using System.Collections;
using CoreLib;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Core_Tests
{
    public class StatRegenTests
    {
        public const float REGEN_RATE = 0.2f;
        public const float REGEN_DELAY = 1;
        public const int MAX_VALUE = 3;
        
        public class TestStat : IRegenStatValue
        {
            private DynamicReactiveProperty<int> _current = new();
            public int MaxValue => MAX_VALUE;

            public int Value
            {
                get => _current.Value;
                set => _current.Value = value;
            }
            public IObservable<(int prevValue, int newValue)> OnValueChanged => _current.OnSwitched;
            public float RegenRate => REGEN_RATE;
            public float RegenResetDelay => REGEN_DELAY;

            public TestStat()
            {
                Value = MaxValue;
            }

            public TestStat(int statValue)
            {
                Value = statValue;
            }
        }

        float TimeToFullyReplenish()
        {
            float timeUntilFull = MAX_VALUE * REGEN_RATE;
            timeUntilFull += Mathf.Epsilon;
            return timeUntilFull;
        }
        [UnityTest]
        public IEnumerator TestRegeneratesAfterReduction()
        {
            var testStat = new TestStat();
            var regen = new Regeneration(testStat);

            testStat.Value = 0;
            yield return new WaitForSeconds(REGEN_DELAY/2f);
            Assert.AreEqual(0, testStat.Value);
            yield return new WaitForSeconds(REGEN_RATE);
            Assert.AreNotEqual(0, testStat.Value);

            float timeUntilFull = TimeToFullyReplenish();
            yield return new WaitForSeconds(timeUntilFull);
            Assert.AreEqual(MAX_VALUE, testStat.Value);
            regen.Dispose();
        }
        [UnityTest]
        public IEnumerator NoRegenAfterDisposal()
        {
            var testStat = new TestStat();
            var regen = new Regeneration(testStat);
            
            testStat.Value = 0;
            regen.Dispose();
            yield return new WaitForSeconds(REGEN_RATE * 2f);
            Assert.AreEqual(0, testStat.Value);
        }
        [UnityTest]
        public IEnumerator TimerResetsOnReduction()
        {
            var testStat = new TestStat(1);
            var regen = new Regeneration(testStat);
            testStat.Value -= 1;
            yield return new WaitForSeconds(REGEN_RATE / 2f);
            Assert.AreEqual(MAX_VALUE-1, testStat.Value);
            testStat.Value -= 1;
            yield return new WaitForSeconds(REGEN_RATE / 2f);
            Assert.AreEqual(MAX_VALUE-2, testStat.Value);
        }
        
        [UnityTest]
        public IEnumerator RegenWaitsForTimer()
        {
            var testStat = new TestStat();
            var regen = new Regeneration(testStat);
            testStat.Value -= 1;
            yield return new WaitForSeconds(REGEN_DELAY);
            yield return null;
            yield return new WaitForSeconds(REGEN_RATE);
            yield return null;
            Assert.AreEqual(MAX_VALUE, testStat.Value);
        }

        [UnityTest]
        public IEnumerator RegenStopsOnReduction()
        {
            var testStat = new TestStat();
            var regen = new Regeneration(testStat);
            testStat.Value -= 2;
            Assert.IsTrue(regen.isRegenPaused);
            yield return new WaitForSeconds(REGEN_DELAY);
            yield return null;
            Assert.IsFalse(regen.isRegenPaused);
            yield return new WaitForSeconds(REGEN_RATE);
            Assert.AreEqual(MAX_VALUE-1, testStat.Value);
            testStat.Value -= 1;
            var v = testStat.Value;
            Assert.IsTrue(regen.isRegenPaused);
            yield return new WaitForSeconds(REGEN_RATE/2f);
            Assert.AreEqual(v, testStat.Value);
        }
    }
}