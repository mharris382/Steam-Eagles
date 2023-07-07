using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class EffectCollider : MonoBehaviour
    {
        [SerializeField] Transform effectPosition;
        public float effectRadius = 1;
        
        
        public Transform EffectPosition => effectPosition ? effectPosition : transform;
        public float EffectRadius => effectRadius;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(EffectPosition.position, EffectRadius);
        }
    }
}