using UniRx;
using UnityEngine;

namespace Damage
{
    public struct StormHandle
    {
        private readonly Storm _storm;
        private StormInstance _instance;
        private CompositeDisposable _disposable;

        private bool _isStarted;
        private bool _isStopped;
        
        public bool IsStarted => _isStarted;
        public bool IsStopped => _isStopped;
        public void Start()
        {
            if (_isStarted)
                return;
            _isStopped = false;
            _disposable = new CompositeDisposable();
            _instance = StormManager.Instance.StartStorm(_storm);
            _instance.StormProgress.First().Subscribe(OnFirst).AddTo(_disposable);
            _instance.StormProgress.Subscribe(OnProgress, OnComplete).AddTo(_disposable);
        }

        void OnFirst(float _)
        {
            _isStarted = true;
            Debug.Log("Storm Started");
        }

        void OnProgress(float t)
        {
            Debug.Log("Storm Progressing");
        }

        void OnComplete()
        {
            _isStopped = true;
            Debug.Log("Storm Complete");
        }
        public void Stop()
        {
            if (_isStopped)
                return;
            _disposable?.Dispose();
            _disposable = null;
            _isStopped = true;
            StormManager.Instance.StopStorm(_instance);
        }
        public StormHandle(Storm storm)
        {
            _storm = storm;
            _isStarted = false;
            _isStopped = false;
            _instance = null;
            _disposable = null;
        }
        
        public StormHandle(int intensity)
        {
            _storm = StormDatabase.Instance.GetStorm(intensity);
            _isStarted = false;
            _isStopped = false;
            _instance = null;
            _disposable = null;
        }
    }
}