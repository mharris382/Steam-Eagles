using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;

namespace Damage.Core
{
    public class StormHandle
    {
        public class Factory : PlaceholderFactory<int, StormHandle> { }
        private readonly int StormHandleID = 0;
        private readonly CoroutineCaller _corotineCaller;

        private  float _startTime;
        private  float _endTime;
        
        
        
        private ReactiveProperty<StormStatus> _status = new ReactiveProperty<StormStatus>(StormStatus.NON_RUNNING);
        private Subject<Unit> _stormStarting = new Subject<Unit>();
        private Subject<Unit> _stormStarted = new Subject<Unit>();
        private Subject<Unit> _stormEnding  = new Subject<Unit>();
        private Subject<Unit> _stormEnded  = new Subject<Unit>();

        public IObservable<StormStatus> OnStatusChanged => _status;
        public IObservable<float> OnStormStarting => _stormStarting.Select(t => _startTime);
        public IObservable<Unit> OnStormStarted => _stormStarted;
        public IObservable<float> OnStormEnding => _stormEnding.Select(t => _endTime);
        public IObservable<Unit> OnStormEnded => _stormEnded;

        public bool IsRunning => ((int)Status) > 0;

        public StormStatus Status
        {
            get => _status.Value;
            set => _status.Value = value;
        }

        private Coroutine _startCoroutine;
        private Coroutine _endCoroutine;


        public float StartTime
        {
            get => _startTime;
            set => _startTime = value;
        }
        
        public float EndTime
        {
            get => _endTime;
            set => _endTime = value;
        }
        
        public StormHandle(int stormHandleID, CoroutineCaller corotineCaller)
        {
            StormHandleID = stormHandleID;
            _corotineCaller = corotineCaller;
            _startTime = 1;
            _endTime = 1;
        }

        public void Start()
        {
            if(_endCoroutine != null) _corotineCaller.StopCoroutine(_endCoroutine);
          _startCoroutine =  _corotineCaller.StartCoroutine(StartStorm());
        }

        public void End()
        {
            if (_startCoroutine != null) _corotineCaller.StopCoroutine(_startCoroutine);
            _endCoroutine = _corotineCaller.StartCoroutine(StopStorm());
        }

        IEnumerator StopStorm()
        {
            Status = StormStatus.ENDING;
            _stormEnding.OnNext(Unit.Default);
            yield return new WaitForSeconds(_endTime);
            Status = StormStatus.ENDED;
            _stormEnded.OnNext(Unit.Default);
        }

        IEnumerator StartStorm()
        {
            _stormStarting.OnNext(Unit.Default);
            Status = StormStatus.STARTING;
            yield return new WaitForSeconds(_startTime);
            Status = StormStatus.STARTED;
            _stormStarted.OnNext(Unit.Default);
            _startCoroutine = null;
        }
    }

    public enum StormStatus
    {
        NON_RUNNING = -1,
        STARTING = 0,
        
        STARTED = 1,
        
        ENDING = 2,
        ENDED = -2
    }
    
    
}