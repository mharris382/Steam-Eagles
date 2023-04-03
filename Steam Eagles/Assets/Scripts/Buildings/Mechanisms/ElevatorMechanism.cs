using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Mechanisms
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SliderJoint2D))]
    public class ElevatorMechanism : SliderJointMechanism
    {
        
        public override float[] GetSaveState()
        {
            throw new System.NotImplementedException();
        }

        public override void LoadSaveState(float[] saveState)
        {
            throw new System.NotImplementedException();
        }

        
         
        [ShowInInspector, ProgressBar(0,1)]
        public float Percent
        {
            get
            {
                var connectedAnchorWS = SliderJoint2D.connectedBody.transform.TransformPoint(SliderJoint2D.connectedAnchor);
                var anchorWS = SliderJoint2D.attachedRigidbody.transform.TransformPoint(SliderJoint2D.anchor);
                var connectedAnchorY = connectedAnchorWS.y;
                var anchorY = anchorWS.y;
                var minY = SliderJoint2D.limits.min + anchorY;
                var maxY = SliderJoint2D.limits.max + anchorY;
                var curY = transform.position.y;
                curY = Mathf.Clamp(minY, maxY, curY);
                return Mathf.InverseLerp(minY, maxY, curY);
            }
        }


        private void OnDrawGizmos()
        {
            var connectedAnchorWS = SliderJoint2D.connectedBody.transform.TransformPoint(SliderJoint2D.connectedAnchor);
            var anchorWS = SliderJoint2D.attachedRigidbody.transform.TransformPoint(SliderJoint2D.anchor);
            var connectedAnchorY = connectedAnchorWS.y;
            var anchorY = anchorWS.y;
            var minY = SliderJoint2D.limits.min + anchorY;
            var maxY = SliderJoint2D.limits.max + anchorY;
            var p0 = transform.position;
            var p1 = p0;
            p1.y += minY;
            p0.y += maxY;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(p0, 0.24f);
            Gizmos.color = Color.Lerp(Color.yellow,Color.white, 0.45f);
            Gizmos.DrawWireSphere(p1, 0.24f);
        }
    }
}