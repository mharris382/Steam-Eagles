using a;
using AI.Enemies;
using AI.Enemies.Systems;
using CoreLib.Structures;
using UnityEngine;
using Zenject;

namespace AI.Bots
{
    public class BotInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ITargetFilter>().To<BotTargetFilter>().AsSingle().NonLazy();
            Container.Bind<ITargetScoreCalculator>().To<BotTargetScoreCalculator>().AsSingle().NonLazy();
            Container.Bind<Health>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<Bot>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<BotAIContext>().FromComponentOn(gameObject).AsSingle();
            Container.BindInterfacesAndSelfTo<BotTargetSelector>().AsSingle().NonLazy();
        }
    }

    public class BotTargetSelector : TargetSelectorBase<BotAIContext>
    {
        public BotTargetSelector(ITargetFinder targets, ITargetScoreCalculator scoreStrategy, ITargetFilter targetFilter, CoroutineCaller coroutineCaller, BotAIContext context) : base(targets, scoreStrategy, targetFilter, coroutineCaller, context)
        {
        }
    }
    public class BotTargetScoreCalculator : ITargetScoreCalculator
    {
        private readonly BotAIContext _aiContext;

        public float CalculateScore(Target target)
        {
            if (target.transform == null) return float.MinValue;
            float score = 0;
            if (_aiContext.HasTarget && target.transform == _aiContext.Target.transform)
                score += _aiContext.targetingConfig.keepTargetSameUtility;
            Vector2 direction = target.transform.position - _aiContext.transform.position;
            Vector2 currentDirection = _aiContext.Self.pivot.right;
            var dot = Vector2.Dot(direction.normalized, currentDirection.normalized);
            return score + dot * _aiContext.targetingConfig.directionUtility;
        }

        public BotTargetScoreCalculator(BotAIContext aiContext)
        {
            _aiContext = aiContext;
        }
    }
}