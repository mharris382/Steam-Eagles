using System;
using System.Collections;
using System.Threading;
using UniRx;
using UnityEngine;

namespace Damage
{
    public class StormInstance : IDisposable
    {
        private readonly Storm _storm;
        private readonly float _timeStarted;
        private readonly Subject<float> _stormProgress = new Subject<float>();

        public IObservable<float> StormProgress => _stormProgress;
        public Storm Config => _storm;
        public StormInstance( Storm storm)
        {
            _storm = storm;
            _timeStarted = Time.realtimeSinceStartup;
            _stormProgress = new Subject<float>();
        }

        public IEnumerator StormUpdate(IObserver<float> observer, CancellationToken ct)
        {
            observer.OnNext(0);
            for (float t = 0; t < _storm.DurationInSeconds; t+=Time.deltaTime)
            {
                if (ct.IsCancellationRequested)
                {
                    observer.OnError(new OperationCanceledException(ct));
                    yield break;
                }
                yield return null;
                observer.OnNext(t/_storm.DurationInSeconds);
            }
            observer.OnNext(1);
            observer.OnCompleted();
        }

        public void CreateStorm()
        {
            Observable.FromCoroutine<float>(StormUpdate).Subscribe(_stormProgress);
        }

        public void Dispose()
        {
            _stormProgress.Dispose();
        }
    }
}