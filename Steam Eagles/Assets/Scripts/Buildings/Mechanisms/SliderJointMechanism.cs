using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Mechanisms
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SliderJoint2D))]
    public abstract class SliderJointMechanism : JointedMechanism
    {
        private SliderJoint2D _sliderJoint2D;
        public SliderJoint2D SliderJoint2D => _sliderJoint2D != null ? _sliderJoint2D : _sliderJoint2D = GetComponent<SliderJoint2D>();
        
        public override Joint2D BuildingJoint => SliderJoint2D;

       
        
        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
    }
}