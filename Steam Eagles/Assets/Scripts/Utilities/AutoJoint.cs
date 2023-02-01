using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.XR;
#endif
namespace Utilities
{
    public class AutoJoint : MonoBehaviour
    {
        public int jointIndex;
        public LayerMask mask;
        public Vector2 targetPosition;
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(AutoJoint))]
    public class AutoJointEditor : Editor
    {
        private SerializedProperty _jointIndexProp;

        private void OnEnable()
        {
            _jointIndexProp = serializedObject.FindProperty("jointIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var autoJoint = target as AutoJoint;
            
            var joints = autoJoint.GetComponents<Joint2D>();
            int index = _jointIndexProp.intValue % joints.Length;
            _jointIndexProp.intValue = index;
            
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            var autoJoint = target as AutoJoint;
            var wp = autoJoint.transform.TransformPoint(autoJoint.targetPosition);
            Handles.color = Color.blue;
            Handles.DrawSolidDisc(wp, Vector3.forward, 0.25f);
            var fmh_46_51_638107055392908464 = Quaternion.identity; var newWP =Handles.FreeMoveHandle(wp, 0.04f, Vector3.zero, Handles.DotHandleCap);
            if (wp != newWP)
            {
                serializedObject.Update();
                autoJoint.targetPosition = autoJoint.transform.InverseTransformPoint(newWP);
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}