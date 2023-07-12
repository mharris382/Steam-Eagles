using CoreLib.Structures;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace AI.Enemies
{
    public class AIContext : MonoBehaviour 
    {
        
        [Inject] public Health Health { get; }
        
        [ShowInInspector, DisplayAsString, ReadOnly]
        public Target Target
        {
            get;
            set;
        }
        public bool HasTarget => Target.transform != null;
        public Vector3 Position => transform.position;
        
        public virtual float GetTargetSelectionRate() => 1f;
    }


    public class AIContext<T> : AIContext where T : MonoBehaviour
    {
        [Inject] public T Self { get; }
    }
}