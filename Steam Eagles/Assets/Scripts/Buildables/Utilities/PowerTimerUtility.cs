using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings
{
    public class PowerTimerUtility
    {
        
        public class Factory : PlaceholderFactory<PowerTimerUtility>
        {
            
        }
        private readonly CoroutineCaller _caller;
        private Coroutine _lastSupplyCheckRoutine;
        private Subject<Unit> _onOffline = new Subject<Unit>();
        private Subject<Unit> _onOnline = new();
        private ReadOnlyReactiveProperty<bool> _isOnline;
        
        public IObservable<bool> IsOnline => _isOnline;

        public float timeWithoutSupplyBeforeConsideredOffline = 5;
        float _lastSupplyTime = 0;
        float TimeSinceLastSupply => Time.time - _lastSupplyTime;

        public PowerTimerUtility(CoroutineCaller caller)
        {
            _isOnline = _onOffline.Select(_ => false).Merge(_onOnline.Select(_ => true)).ToReadOnlyReactiveProperty();
            _caller = caller;
        }

        public void OnPowerSupplied(float amount)
        {
            if (amount == 0) return;
            Debug.Assert(amount > 0, "negatives not allowed");
            _onOnline.OnNext(Unit.Default);
            _lastSupplyTime = Time.time;
            if(_lastSupplyCheckRoutine != null) _caller.StopCoroutine(_lastSupplyCheckRoutine);
            _lastSupplyCheckRoutine = _caller.StartCoroutine(LastSupplyCheckRoutine());
        }
        
        IEnumerator LastSupplyCheckRoutine()
        {
            yield return new WaitForSeconds(timeWithoutSupplyBeforeConsideredOffline);
            if (TimeSinceLastSupply > timeWithoutSupplyBeforeConsideredOffline)
            {
                _onOffline.OnNext(Unit.Default);
            }
        }
    }
}