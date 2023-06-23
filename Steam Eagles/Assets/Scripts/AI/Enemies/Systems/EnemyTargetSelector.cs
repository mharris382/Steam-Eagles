using System;
using System.Collections;
using System.Linq;
using AI.Enemies.Installers;
using UniRx;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Systems
{
    public class EnemyTargetSelector : IInitializable, IDisposable
    {
        private readonly ITargetFinder _targetFinder;
        private readonly ITargetScoreCalculator _targetScoreCalculator;
        private readonly ITargetFilter _targetFilter;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly EnemyAIContext _context;
        private CompositeDisposable _cd;
        public EnemyTargetSelector(ITargetFinder targetFinder, ITargetScoreCalculator targetScoreCalculator, ITargetFilter targetFilter, CoroutineCaller coroutineCaller,
            EnemyAIContext context)
        {
            _targetFinder = targetFinder;
            _targetScoreCalculator = targetScoreCalculator;
            _targetFilter = targetFilter;
            _coroutineCaller = coroutineCaller;
            _context = context;


            _cd = new();
        }

        public void Initialize()
        {
            _context.Health.onRespawn.AsObservable().Subscribe(_ => StartSelection());
            StartSelection();
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
                yield return new WaitForSeconds(_context.Config.targetSwitchInterval);
            }
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

        public void Dispose()
        {
            _cd.Dispose();
        }
    }
}