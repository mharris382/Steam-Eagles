using UnityEngine;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class TestInputForce : DynamicForceInputObject
    {
        public Vector2 force = Vector2.right;

        public override Vector2 GetInputForce()
        {
            return force;
        }
    }
}