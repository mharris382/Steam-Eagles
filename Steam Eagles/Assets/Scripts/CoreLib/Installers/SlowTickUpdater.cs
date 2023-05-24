using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;
using Zenject;

public class SlowTickUpdater : IInitializable, IDisposable
{
    private readonly SlowTickConfig _slowTickConfig;
    private readonly CoroutineCaller _coroutineCaller;
    private readonly List<ISlowTickable> _slowTickables;
    private readonly List<IExtraSlowTickable> _extraSlowTickables;
    private readonly Queue<IExtraSlowTickable> _extraSlowRemovalQueue = new Queue<IExtraSlowTickable>();
    private readonly Queue<ISlowTickable> _slowRemovalQueue = new Queue<ISlowTickable>();
    private readonly Queue<IExtraSlowTickable> _extraSlowAddQueue = new Queue<IExtraSlowTickable>();
    private readonly Queue<ISlowTickable> _slowAddQueue = new Queue<ISlowTickable>();
    
    
    private CompositeDisposable _cd;
    public SlowTickUpdater(SlowTickConfig slowTickConfig, CoroutineCaller coroutineCaller, List<ISlowTickable> slowTickables, List<IExtraSlowTickable> extraSlowTickables)
    {
        _slowTickConfig = slowTickConfig;
        _coroutineCaller = coroutineCaller;
        _slowTickables = slowTickables;
        _extraSlowTickables = extraSlowTickables;
        _cd = new CompositeDisposable();
    }

    private struct CancelCoroutineDisposables : IDisposable
    {
        private readonly CoroutineCaller _caller;
        private readonly Coroutine _cd;
        public CancelCoroutineDisposables(CoroutineCaller caller, Coroutine cd)
        {
            _caller = caller;
            _cd = cd;
        }
        public void Dispose()
        {
            _caller.StopCoroutine(_cd);   
        }
    }
    public void Initialize()
    {
        
        void ProcessExtraSlowAddRequest(ExtraSlowTickAddRequest request)
        {
            if(request.ExtraSlowTickable != null)
            {
                _extraSlowAddQueue.Enqueue(request.ExtraSlowTickable);
                Log(request.ToString);
            }
        }

        void ProcessSlowAddRequest(SlowTickAddRequest request)
        {
            if (request.SlowTickable != null)
            {
                _slowAddQueue.Enqueue(request.SlowTickable);
                Log(request.ToString);
            }
        }

        void ProcessExtraSlowRemoveRequest(ExtraSlowTickRemovalRequest request)
        {
            if (request.ExtraSlowTickable != null)
            {
                _extraSlowRemovalQueue.Enqueue(request.ExtraSlowTickable);
                Log(request.ToString);
            }
        }

        void ProcessSlowRemoveRequest(SlowTickRemovalRequest request)
        {
            if (request.SlowTickable != null)
            {
                _slowRemovalQueue.Enqueue(request.SlowTickable);
                Log(request.ToString);
            }
        }
        
        new CancelCoroutineDisposables(_coroutineCaller, _coroutineCaller.StartCoroutine(DoSlowTick())).AddTo(_cd);
        new CancelCoroutineDisposables(_coroutineCaller, _coroutineCaller.StartCoroutine(DoExtraSlowTick())).AddTo(_cd);
        MessageBroker.Default.Receive<ExtraSlowTickAddRequest>().Subscribe(ProcessExtraSlowAddRequest).AddTo(_cd);
        MessageBroker.Default.Receive<SlowTickAddRequest>().Subscribe(ProcessSlowAddRequest).AddTo(_cd);
        MessageBroker.Default.Receive<SlowTickRemovalRequest>().Subscribe(ProcessSlowRemoveRequest).AddTo(_cd);
        MessageBroker.Default.Receive<ExtraSlowTickRemovalRequest>().Subscribe(ProcessExtraSlowRemoveRequest).AddTo(_cd);
        Log($"Initialized Slow Tickable with slow tick rate of {_slowTickConfig.slowTickRate} and extra slow tick rate of {_slowTickConfig.extraSlowTickRate}\n" +
            $"Slow Tickable Count = {_slowTickables.Count()}\t Extra Slow Tickable Count = {_extraSlowTickables.Count()}");
    }
   
    IEnumerator DoSlowTick()
    {
        float timeLastTicked = 0;
        while (true)
        {
            AddRemoveSlowTickables();
            var waitTime = _slowTickConfig.slowTickRate;
            timeLastTicked = Time.time;
            foreach (var slowTickable in _slowTickables)
            {
                slowTickable.SlowTick(waitTime);
            }
            yield return new WaitForSeconds(waitTime - (Time.time - timeLastTicked));
            Log("SlowTick");
        }
    }
    IEnumerator DoExtraSlowTick()
    {
        float timeLastTicked = 0;
        while (true)
        {
            AddRemoveExtaSlowTickables();
            var waitTime = _slowTickConfig.extraSlowTickRate;
            timeLastTicked = Time.time;
            foreach (var slowTickable in _extraSlowTickables)
            {
                if(slowTickable != null)
                {
                    slowTickable.ExtraSlowTick(waitTime);
                }
            }
            yield return new WaitForSeconds(waitTime - (Time.time - timeLastTicked));
            Log("ExtraSlowTick");
        }
    }
    private void AddRemoveSlowTickables()
    {
        while (_extraSlowAddQueue.Count > 0)
        {
            var t = _slowAddQueue.Dequeue();
            if (_slowTickConfig.safetyChecksOn && _slowTickables.Contains(t))
            {
                Debug.LogError("Duplicate ExtraSlowTickable added to SlowTickUpdater");
                continue;
            }

            _slowTickables.Add(t);
        }

        while (_slowRemovalQueue.Count > 0)
        {
            var t = _slowRemovalQueue.Dequeue();
            if (_slowTickConfig.safetyChecksOn && !_slowTickables.Contains(t))
            {
                Debug.LogError("ExtraSlowTickable removed from SlowTickUpdater that was never added");
                continue;
            }
            _slowTickables.Remove(t);
        }
    }
    private void AddRemoveExtaSlowTickables()
    {
        while (_extraSlowAddQueue.Count > 0)
        {
            var t = _extraSlowAddQueue.Dequeue();
            if (_slowTickConfig.safetyChecksOn && _extraSlowTickables.Contains(t))
            {
                Debug.LogError("Duplicate ExtraSlowTickable added to SlowTickUpdater");
                continue;
            }

            _extraSlowTickables.Add(t);
        }

        while (_extraSlowRemovalQueue.Count > 0)
        {
            var t = _extraSlowRemovalQueue.Dequeue();
            if (_slowTickConfig.safetyChecksOn && !_extraSlowTickables.Contains(t))
            {
                Debug.LogError("ExtraSlowTickable removed from SlowTickUpdater that was never added");
                continue;
            }

            _extraSlowTickables.Remove(t);
        }
    }

    public void Dispose()
    {
        _cd?.Dispose();
        Log("SlowTickUpdater Disposed");
    }
    

    void Log(string msg) => _slowTickConfig.Log(msg);

    void Log(Func<string> msgFunc) => _slowTickConfig.Log(msgFunc);
}


[Serializable]
public class SlowTickConfig
{
    public float slowTickRate = 1;
    public float extraSlowTickRate = 5;

    [Tooltip("if true, will check for duplicates and throw an error if found. If false, will not check for duplicates and will not throw an error if found.")]
    public bool safetyChecksOn = true;
    
    public LogLevel logLevel = LogLevel.VERBOSE;
    public enum LogLevel
    {
        NONE,
        VERBOSE,
        VERBOSE_WITH_TESTS
    }
    
    public void Log(string msg)
    {
        if (logLevel == SlowTickConfig.LogLevel.VERBOSE)
        {
            Debug.Log(msg);
        }
    }

    public void Log(Func<string> msgFunc)
    {
        if (logLevel == SlowTickConfig.LogLevel.VERBOSE)
        {
            Debug.Log(msgFunc());
        }
    }
}

