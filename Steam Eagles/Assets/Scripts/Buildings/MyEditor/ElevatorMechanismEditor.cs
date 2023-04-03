using System;
using Buildings.Mechanisms;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    [CustomEditor(typeof(ElevatorMechanism))]
    public class ElevatorMechanismEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            var mechanism = (ElevatorMechanism) target;

            if (mechanism.BuildingJoint.connectedBody == null)
            {
                if (mechanism.BuildingMechanisms == null)
                {
                    Debug.LogError("No BuildingMechanisms found on " + mechanism.name);
                    return;
                }
            }
            
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            var mechanism = (ElevatorMechanism) target;
            
            if(mechanism.BuildingJoint.connectedBody == null)
                mechanism.BuildingMechanisms.LinkJointsImmediately();

            var joint = mechanism.BuildingJoint as SliderJoint2D;
            var mechanismTransform = mechanism.transform;
            var mechanismPositionWS = (Vector2)mechanismTransform.position;
            var mechanismPositionLS = mechanismTransform.InverseTransformPoint(mechanismPositionWS);
            
            //elevators only move in the y direction
            var anchor = joint.anchor;
            var connectedAnchor = joint.connectedAnchor;
            anchor.x = mechanismPositionLS.x;
            connectedAnchor.x = mechanism.BuildingMechanisms.transform.InverseTransformPoint(mechanismPositionWS).x;
            joint.anchor = anchor;
            joint.connectedAnchor = connectedAnchor;
        }
    }
}