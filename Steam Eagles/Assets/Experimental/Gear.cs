using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Experimental
{
    public class Gear : MonoBehaviour
    {
        public Gear parentGear;
        [HideInInspector]
        public List<Gear> childGear;

        
        public float radius = 2;
        public bool axelConnection = false;
        public float connectAngle = 15;
        public bool IsRoot()
        {
            return GetComponent<Gears>() != null;
        }

        public Vector3 Center => transform.position;


        public Vector3 GetChildPosition(float childRadius, float childAttachAngle)
        {
            Vector3 dir =  Quaternion.Euler(0, 0, childAttachAngle) * Vector3.right;
            var offset = dir * (radius + childRadius);
            return Center + offset;
        }

       
    }




#if UNITY_EDITOR
    [CustomEditor(typeof(Gear))]
    public class GearEditor : Editor
    {
        Gear Gear => target as Gear;

        

        public float StartRotationAngle
        {
            get => Gear.transform.eulerAngles.z;
            set
            {
                
                UpdateStartRotation(value);
            }
        }
        
        

        private void UpdateStartRotation(float value)
        {
            var current = Gear;
            float curRotation = value;
            current.transform.rotation = Quaternion.Euler(0, 0, value);
            while (current.parentGear != null)
            {
                UpdateParent(current, curRotation);
            }
            UpdateChildren(Gear, value);
        }

        private void UpdateChildren(Gear parent, float newParentRotation)
        {
            var childRotation = -newParentRotation;
            parent.transform.rotation = Quaternion.Euler(0, 0, childRotation);
            foreach (var gear in parent.childGear)
            {
                if (gear.axelConnection) continue;
                UpdateChildren(gear, childRotation);
            }
        }

        public Gear UpdateParent(Gear child, float childRotation)
        {
            if (child.IsRoot()) return null;
            var newParentRotation = -childRotation;
            child.parentGear.transform.rotation = Quaternion.Euler(0, 0, newParentRotation);
            return child.parentGear;
        }
        
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying==false)
            {
                EditorGUI.BeginChangeCheck();
                float rotation = EditorGUILayout.FloatField("Current Rotation", StartRotationAngle);
                if (EditorGUI.EndChangeCheck())
                {
                    StartRotationAngle = rotation;
                }
            }

            if (GUILayout.Button("Create Child"))
            {
                var go = Instantiate(Gear, Gear.GetChildPosition(Gear.radius, Random.Range(0f, 360f)),
                    Quaternion.identity);
                go.transform.SetParent(Gear.transform.parent);
                var childGear = go.GetComponent<Gear>();
                Gear.childGear.Add(childGear);
                childGear.parentGear = Gear;
            }
            base.OnInspectorGUI();

            GUILayout.Space(5);
            
          
        }

        void DrawHandles(Gear gear, bool findRoot =true)
        {
            var root = gear;
            if (findRoot)
            {
                while (root.parentGear != null)
                {
                    root = root.parentGear;
                }
            }
            Handles.color = Color.Lerp(Color.blue, Color.cyan, 0.5f);
            Handles.DrawWireDisc(root.transform.position, Vector3.forward, root.radius);
            foreach (var gear1 in root.childGear)
            {
                var p0 = gear1.Center;
                var p1 = gear.Center;
                Handles.DrawDottedLine(p0, p1, HandleUtility.GetHandleSize(p1) * 0.3f);
                DrawHandles(gear1, false);
            }
            /*
            Handles.DrawWireDisc(gear.transform.position, Vector3.forward, gear.radius);
            DrawHandlesParents(gear);
            
            foreach (var gear1 in gear.childGear)
            {
                var p0 = gear1.Center;
                var p1 = gear.Center;
                Handles.DrawDottedLine(p0, p1, HandleUtility.GetHandleSize(p1) * 0.3f);
                DrawHandles(gear1);
            }*/
        }

        void DrawHandlesParents(Gear gear)
        {
            if (gear.parentGear != null)
            {
                gear = gear.parentGear;
                Handles.color = Color.Lerp(Color.blue, Color.green, 0.5f);
                Handles.DrawWireDisc(gear.transform.position, Vector3.forward, gear.radius);
                DrawHandlesParents(gear);
            }
            
            foreach (var gear1 in gear.childGear)
            {
                var p0 = gear1.Center;
                var p1 = gear.Center;
                Handles.DrawDottedLine(p0, p1, HandleUtility.GetHandleSize(p1) * 0.3f);
            }
        }
        private void OnSceneGUI()
        {
            var gear = target as Gear;
            DrawHandles(Gear);
            if (Gear.parentGear!=null)
            {
                UpdateChildPosition(Gear);
            }
        }

        private void UpdateChildPosition(Gear gear)
        {
            if (gear.parentGear == null)
            {
                return;
            }

            if (gear.axelConnection)
            {
                gear.transform.position = gear.parentGear.Center;
            }
            else
            {
                var gearPos = gear.parentGear.GetChildPosition(gear.radius, gear.connectAngle);
                gear.transform.position = gearPos;
            }
            foreach (var gear1 in gear.childGear)
            {
                UpdateChildPosition(gear1);
            }
        }
    }
    
#endif
}