using System;
using System.Collections;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Systems
{
    public abstract class TargetSelectorBase<T> : IInitializable, IDisposable where T : AIContext
    {
        
        private CompositeDisposable _cd = new();
        private readonly ITargetFinder _targetFinder;
        private readonly ITargetScoreCalculator _targetScoreCalculator;
        private readonly ITargetFilter _targetFilter;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly T _context;
        
        protected T Context => _context;

        public TargetSelectorBase(ITargetFinder targets,
            ITargetScoreCalculator scoreStrategy,
            ITargetFilter targetFilter, 
            CoroutineCaller coroutineCaller, T context)
        {
            _targetFilter = targetFilter;
            _coroutineCaller = coroutineCaller;
            _context = context;
            _targetFinder = targets;
            _targetScoreCalculator = scoreStrategy;
        }
        public void Initialize()
        {
            _context.Health.onRespawn.AsObservable().Subscribe(_ => StartSelection());
        }
        private void StartSelection()
        {
            _coroutineCaller.StartCoroutine(SelectTarget());
        }
        private IEnumerator SelectTarget()
        {
            while (!_context.Health.IsDead)
            {
                UpdateTargetSelection();
                yield return new WaitForSeconds(_context.GetTargetSelectionRate());
            }
            
            void UpdateTargetSelection()
            {
                var targets = _targetFinder.GetTargets().Where(_targetFilter.Filter).OrderByDescending(_targetScoreCalculator.CalculateScore).ToList();
                if (targets.Count == 0)
                {
                    _context.Target = default;
                    return;
                }
                _context.Target = targets[0];
            }
        }
        public void Dispose()
        {
            _cd.Dispose();
        }
    }
}