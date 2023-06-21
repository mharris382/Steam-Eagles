using UnityEngine;

namespace AI.Enemies
{
    public abstract class FlyingEnemyBase : MonoBehaviour
    {
        ITargetFinder _targetFinder;
    }
}