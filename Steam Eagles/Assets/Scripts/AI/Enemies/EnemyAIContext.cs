using CoreLib.Structures;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace AI.Enemies.Installers
{
    public class EnemyAIContext : MonoBehaviour
    {
        [Inject] public Enemy Self { get; }
        [Inject] public Health Health { get; }
        [Inject] public Enemy.Config Config { get; }

        [ShowInInspector, DisplayAsString, ReadOnly]
        public Target Target
        {
            get;
            set;
        }

        public bool HasTarget => Target.transform != null;
        public Vector3 Position => Self.transform.position; 
    }
    
    
    
}