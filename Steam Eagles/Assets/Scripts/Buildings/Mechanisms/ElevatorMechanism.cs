using UnityEngine;

namespace Buildings.Mechanisms
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SliderJoint2D))]
    public class ElevatorMechanism : JointedMechanism
    {
        private SliderJoint2D _sliderJoint2D;
        public SliderJoint2D SliderJoint2D => _sliderJoint2D != null ? _sliderJoint2D : _sliderJoint2D = GetComponent<SliderJoint2D>();
        public override float[] GetSaveState()
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSaveState(float[] saveState)
        {
            throw new System.NotImplementedException();
        }

        public override Joint2D BuildingJoint => SliderJoint2D;
    }
}