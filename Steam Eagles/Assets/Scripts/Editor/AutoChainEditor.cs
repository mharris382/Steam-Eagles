using System;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AutoChain : MonoBehaviour
    {
        [Range(0.01f, 1)]
        public float handleScale = 0.04f;
        public Color color = Color.magenta;
        
    }
    [CustomEditor(typeof(AutoChain))]
    public class AutoChainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            var chain = target as AutoChain;
            if(chain.transform.childCount < 2)
                return;
            Transform childStart = chain.transform.GetChild(0);
            Transform childEnd = chain.transform.GetChild(chain.transform.childCount - 1);
            Vector3 startPosition = childStart.position;
            Vector3 endPosition = childEnd.position;
            float size = HandleUtility.GetHandleSize(startPosition) * chain.handleScale;
            Handles.color = chain.color;
            //draw free move handle for start point and end point
            Vector3 newStartPosition = Handles.FreeMoveHandle(startPosition, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
            Vector3 newEndPosition = Handles.FreeMoveHandle(endPosition, Quaternion.identity, size, Vector3.zero, Handles.SphereHandleCap);
            
            //check if start point or end point is moved
            if (newStartPosition != startPosition)
            {
                Undo.RecordObject(childStart, "Move Start Point");
                childStart.position = newStartPosition;
                startPosition = newStartPosition;
            }
            if (newEndPosition != endPosition)
            {
                Undo.RecordObject(childEnd, "Move End Point");
                childEnd.position = newEndPosition;
                endPosition = newEndPosition;
            }

            for (int i = 1; i < chain.transform.childCount; i++)
            {
                Transform child0 = chain.transform.GetChild(i - 1);
                Transform child1 = chain.transform.GetChild(i);
                Vector2 p0 = child0.position;
                Vector2 p1 = child1.position;
                var color = chain.color;
                color.a = 1 - (float)i / chain.transform.childCount;
                Handles.color = color;
                Handles.DrawLine(p0, p1);
            }
        }
    }
}