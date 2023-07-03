using AI.Enemies.Installers;

namespace AI.Enemies.Systems
{
    public class EnemyTargetSelector : TargetSelectorBase<EnemyAIContext>
    {
        public EnemyTargetSelector(ITargetFinder targets, ITargetScoreCalculator scoreStrategy, ITargetFilter targetFilter, CoroutineCaller coroutineCaller, EnemyAIContext context) : base(targets, scoreStrategy, targetFilter, coroutineCaller, context){ }
    }
}