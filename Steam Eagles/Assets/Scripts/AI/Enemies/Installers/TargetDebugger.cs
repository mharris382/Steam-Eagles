using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib.Structures;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Installers
{
    public class TargetDebugger : MonoBehaviour
    {
        [Required] public SpriteRenderer visualPrefab;
        public float updateRate = 0.1f;
        private Subject<Target> _targetSubject = new();
        private ITargetFinder _targetFinder;
        private Dictionary<Target, SpriteRenderer> _visuals = new Dictionary<Target, SpriteRenderer>();
        private ReadOnlyReactiveProperty<IList<Target>> _currentTargetsSet;

        [Inject] void InjectMe(ITargetFinder targetFinder)
        {
            _targetFinder = targetFinder;
        }
        private void Awake()
        {
            var targetStream = _targetSubject.Buffer(TimeSpan.FromSeconds(updateRate));
            targetStream.Subscribe(targets =>
            {
                lock (_visuals)
                {
                    foreach (var target in targets)
                    {
                        if (_visuals.ContainsKey(target)) continue;
                        var visual = Instantiate(visualPrefab, target.transform.position, Quaternion.identity);
                        _visuals.Add(target, visual);
                    }
                    foreach (var target in _visuals.Keys.ToArray())
                    {
                        if (targets.Contains(target)) continue;
                        if(_visuals[target]!=null)
                            Destroy(_visuals[target].gameObject);
                        _visuals.Remove(target);
                    }
                }
            });
        }

        void Update()
        {
            lock (_visuals)
            {
                foreach (var kvp in _visuals)
                {
                    var sr = kvp.Value;
                    var target = kvp.Key;
                    sr.transform.position = target.transform.position;
                }
            }
        }
        

        private void OnEnable()
        {
            StartCoroutine(nameof(CheckForTargets));
        }

        private void OnDisable()
        {
            StopCoroutine(nameof(CheckForTargets));
        }

        private IEnumerator CheckForTargets()
        {
            while (enabled)
            {
                if (_targetFinder == null)
                {
                    yield return null;
                    continue;
                }

                yield return new WaitForSeconds(updateRate);
                foreach (var target in _targetFinder.GetTargets()) _targetSubject.OnNext(target);
            }
        }
    }
}