using System;
using AI.Enemies.Systems;
using CoreLib.Structures;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Installers
{
    public class EnemyInstaller : MonoInstaller
    {
        enum ScoreStrategy
        {
            EQUAL,
            DISTANCE
        }

        [SerializeField] private bool filterByDistance;
        [SerializeField] private ScoreStrategy scoreStrategy;
        public override void InstallBindings()
        {
            Container.Bind<Health>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<Enemy>().FromComponentOn(gameObject).AsSingle();
            Container.Bind<Enemy.Config>().FromMethod(GetConfig).AsSingle();
            Container.Bind<EnemyAIContext>().FromComponentOn(gameObject).AsSingle().NonLazy();
            
            Container.Bind<ITargetFilter>().To(filterByDistance ? typeof(NullFilter) : typeof(DistanceFilter)).AsSingle();
            switch (scoreStrategy)
            {
                case ScoreStrategy.EQUAL:
                    Container.Bind<ITargetScoreCalculator>().To<EqualScorer>().AsSingle();
                    break;
                case ScoreStrategy.DISTANCE:
                    Container.Bind<ITargetScoreCalculator>().To<DistanceScorer>().AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Container.BindInterfacesTo<EnemyTargetSelector>().AsSingle().NonLazy();
        }

        Enemy.Config GetConfig(InjectContext context)
        {
            var enemy  = context.Container.Resolve<Enemy>();
            return enemy.EnemyConfig;
        }

        struct NullFilter : ITargetFilter
        {
            public bool Filter(Target target) => false;
        }

        class DistanceFilter : ITargetFilter
        {
            private readonly Enemy.Config _config;
            private readonly Transform _transform;

            DistanceFilter(Enemy.Config config, Enemy enemy)
            {
                _config = config;
                _transform = enemy.transform;
            }
            public bool Filter(Target target)
            {
                if (target.transform == null) return false;
                float distSquar = Vector3.SqrMagnitude(target.transform.position - _transform.position);
                return distSquar < _config.maxEngagementDistance * _config.maxEngagementDistance;
            }
        }
        
        abstract class ScorerBase : ITargetScoreCalculator
        {
            private readonly EnemyAIContext _aiContext;

            protected EnemyAIContext Context => _aiContext;
            public ScorerBase(EnemyAIContext aiContext)
            {
                _aiContext = aiContext;
            }
            public abstract float CalculateScore(Target target);
        }

        class DistanceScorer : ScorerBase
        {
            public override float CalculateScore(Target target)
            {
                var dist = Vector3.Distance(target.transform.position, this.Context.Position);
                var maxDist = this.Context.Config.maxEngagementDistance;
                dist = Mathf.Clamp(dist, 0, maxDist);
                return 1 - dist / maxDist;
            }

            public DistanceScorer(EnemyAIContext aiContext) : base(aiContext)
            {
            }
        }

        class EqualScorer : ITargetScoreCalculator
        {
            public float CalculateScore(Target target)
            {
                return 1;
            }
        }
    }
    
    
}