using System;
using CoreLib;

namespace Tests.Core_Tests
{
    public class TestStat : IRegenStatValue
    {
        private DynamicReactiveProperty<int> _current = new();
        
        private readonly bool _regenEnabled;
        private float _regenRate;
        private int _maxValue;
        private float _regenDelay;
        public int MaxValue => _maxValue;

        public int Value
        {
            get => _current.Value;
            set => _current.Value = value;
        }
        public IObservable<(int prevValue, int newValue)> OnValueChanged => _current.OnSwitched;
        public float RegenRate => _regenEnabled ? _regenRate : 0;
        public float RegenResetDelay => _regenDelay;

        public TestStat(int current = 10, int max = 10, bool regenEnabled = false)
        {
            _regenEnabled = regenEnabled;
            _maxValue = max;
            Value = current;
        }

        public TestStat(int statValue)
        {
            Value = statValue;
        }
    }
}