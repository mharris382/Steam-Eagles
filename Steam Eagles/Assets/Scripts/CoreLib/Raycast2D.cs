using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class Raycast2D : MonoBehaviour
{
    public LayerMask layerMask;
    public float distance = 100;

    public bool debug = false;

    public RaycastHit2D Hit2D
    {
        get;
        private set;
    }
    
    public void FixedUpdate()
    {
        var t = transform;
        var origin = t.position;
        var direction = t.right;
        var contactFilter = new ContactFilter2D()
        {
            layerMask = this.layerMask,
            useLayerMask = true,
            
            useDepth = false,
            useTriggers = false,
            useNormalAngle = false,
            useOutsideDepth = false,
            useOutsideNormalAngle = false
        };
        Hit2D = Physics2D.Raycast(origin, direction, distance, layerMask);
    }

    public void OnDrawGizmos()
    {
       // if (Application.isPlaying)
       // {
       if (!debug) return;
            Gizmos.color = Hit2D ? Color.green : Color.red;
            var t = transform;
            var p0 = (Vector2) t.position;
            var p1 = !Hit2D ? (p0 + (Vector2)(t.right * distance)) : Hit2D.point;
            
            Gizmos.DrawLine(p0,p1);
      //  }
    }

    public bool CheckForHit() => Hit2D;
}

#if UNITY_EDITOR
[CanEditMultipleObjects()]
[CustomEditor(typeof(Raycast2D))]
public class Raycast2DEditor : Editor
{
    private SerializedProperty _layerMaskProperty;
    private SerializedProperty _distanceProperty;
    
    private void OnEnable()
    {
        _layerMaskProperty = serializedObject.FindProperty("layerMask");
        _distanceProperty = serializedObject.FindProperty("distance");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        var raycast = target as Raycast2D;
        var dir = raycast.transform.right;
        var origin = raycast.transform.position;
        var dist = _distanceProperty.floatValue;

        var handlePos = origin + (dir * dist);
        Handles.color = Color.green;
        Handles.DrawLine(origin, handlePos);
        
        
        EditorGUI.BeginChangeCheck();
        
        var pos = Handles.FreeMoveHandle(handlePos, Quaternion.identity, 0.05f * HandleUtility.GetHandleSize(origin), Vector3.zero,
            Handles.DotHandleCap);
        
        if (EditorGUI.EndChangeCheck())
        {
            dist =Vector2.Distance(origin, pos);
            if (Mathf.Abs(dist) > Mathf.Epsilon)
            {
                Undo.RecordObject(target, "Set Raycast Distance");
                raycast.distance = dist;
            }
           
        }
    }
}
#endif
