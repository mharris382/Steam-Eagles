using System;
using System.Collections;
using UnityEngine;

namespace Tools.BuildTool
{
    public class BuildActionHandle
    {
        private readonly MonoBehaviour _caller;
        private readonly float _duration;
        private readonly int _intervals;
        public bool IsActionComplete { get;  private set; }
        public bool IsStarted { get; private set; }
        public event Action<int> onBuildActionInterval;
        public event Action onBuildActionCompleted;

        public BuildActionHandle(MonoBehaviour caller, float duration = 0.2f, int intervals = 3)
        {
            _caller = caller;
            _duration = duration;
            _intervals = intervals;
            IsActionComplete = false;
            IsStarted = false;
            onBuildActionCompleted += () => IsActionComplete = true;
        }

        public void StartAction()
        {
            if (IsStarted)
            {
                Debug.LogError("BuildActionHandle: Action already started!");
                return;
            }
            IsStarted = true;
            _caller.StartCoroutine(DoBuildAction(_duration, _intervals,
                (t) => onBuildActionInterval?.Invoke(t), 
                () => onBuildActionCompleted?.Invoke())
            );
        }

        IEnumerator DoBuildAction(float duration, int intervals, Action<int> onInterval, Action onBuildCompleted)
        {
            float durationPerInterval = duration / intervals;
            for (int i = 0; i < intervals; i++)
            {
                onInterval?.Invoke(i);
                yield return new WaitForSeconds(durationPerInterval);
            }
        }
    }
}