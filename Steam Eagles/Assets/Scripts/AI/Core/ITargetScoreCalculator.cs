using CoreLib.Structures;

namespace AI.Enemies.Systems
{
    public interface ITargetScoreCalculator
    {
        float CalculateScore(Target target);
    }


    public interface ITargetFilter
    {
        bool Filter(Target target);
    }
}