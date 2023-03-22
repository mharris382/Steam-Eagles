using System;
using System.Collections.Generic;
using NaughtyAttributes;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace PhysicsFun.SoftBody2D
{
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class SoftBody2D : MonoBehaviour
    {
       

        public bool autoPosition = true;
        [Range(3, 25)] public int numBodies = 4;
        
        [Header("Spring Distance")]
        public bool overrideDistance = false;
        [ShowIf("overrideDistance")] public float distance = 1f;

        [Header("Spring Settings")]
        public float springFrequency = 3;
        [Range(0,1)] public float springDamping = 0f;
        
        [Header("Radius")] 
        public bool uniformRadius = true;
        [ShowIf(nameof(uniformRadius))] public float bodyRadius = 0.1f;
        public float padding = 0.5f;

        [Header("Middle Spring")] 
        public bool overrideSpringMiddle = false;
        [ShowIf(nameof(overrideSpringMiddle))] public float springFrequencyMiddle = 3;
        [ShowIf(nameof(overrideSpringMiddle))] public float springDistanceMiddle = 4;
        [ShowIf(nameof(overrideSpringMiddle)), Range(0,1)] public float springDampingMiddle = 0;

        
        
        private Rigidbody2D _rb;
        private CircleCollider2D _circle;
        public Rigidbody2D centerBody => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
        public CircleCollider2D circle => _circle ? _circle : _circle = GetComponent<CircleCollider2D>();
    
        public void UpdateBodies()
        {
            if (!Application.isPlaying)
            {
                INTERNAL_UpdateBodies_EDITOR();
            }
            else
            {
                throw new Exception("Call this method only in editor mode");
            }
        }

        
        [Button()]
        internal void INTERNAL_UpdateBodies_EDITOR()
        {
            if (transform.childCount <= 1) return;
            for (int i = 0; i < transform.childCount; i++)
            {
                
                var child0 = transform.GetChild(i > 0 ? (i - 1) : transform.childCount-1).GetComponent<SoftBody2DCollider>();
                var child1 = transform.GetChild(i).GetComponent<SoftBody2DCollider>();
                
                var springs0 = child0.GetComponents<SpringJoint2D>();
                var springs1 = child1.GetComponents<SpringJoint2D>();
                
                Debug.Assert(springs0.Length >= 3);
                Debug.Assert(springs1.Length >= 3);
                
                SetupColliders(child0, child1);
            }
            if(autoPosition) AutoPositionBodies();
        }

        private void INTERNAL_UpdateBodies()
        {
            var t = transform;
            for (int i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i).GetComponent<SoftBody2DCollider>();
                if (child == null) continue;
                child.GetSpringToNextBody().frequency = child.GetSpringToPrevBody().frequency = springFrequency;
                child.GetSpringToNextBody().dampingRatio = child.GetSpringToPrevBody().dampingRatio = springDamping;
                if (overrideDistance)
                {
                    
                    child.GetSpringToNextBody().distance = child.GetSpringToPrevBody().distance = distance;
                }
                
            }
        }
        

        private void SetupColliders(SoftBody2DCollider child0, SoftBody2DCollider child1)
        {
            SetupCollider(child0);
            SetupCollider(child1);
            
            var s0 = child0.GetSpringToNextBody();
            var s1 = child1.GetSpringToPrevBody();
            
            s0.connectedBody = child1.rigidbody;
            s1.connectedBody = child0.rigidbody;
            
            var s02 = child0.GetSpringToMiddle();
            var s12 = child1.GetSpringToMiddle();
            
            s0.frequency = s1.frequency = this.springFrequency;
            s0.dampingRatio = s1.dampingRatio = this.springDamping;
            if(_rb == null)
                this._rb = GetComponent<Rigidbody2D>();
            s02.connectedBody = s12.connectedBody = this._rb;
        }


        private void SetupCollider(SoftBody2DCollider child0)
        {
            child0.circleCollider2D.radius = this.bodyRadius;
            var middle = child0.GetSpringToMiddle();
            middle.frequency = springFrequencyMiddle;
            if (middle.autoConfigureDistance == false)
                middle.distance = this.springDistanceMiddle;
        }

        
        public void UpdateBody(SoftBody2DCollider body)
        {
            string bodyName = $"{name} Body {body.transform.GetSiblingIndex()}";
            body.name = bodyName;
            body.circleCollider2D.radius = bodyRadius;
            body.GetSpringToMiddle().connectedBody = this.centerBody;
            var neighborSprings = !overrideSpringMiddle ?
                new[] { body.GetSpringToNextBody(), body.GetSpringToPrevBody() , body.GetSpringToMiddle()} : 
                new[] { body.GetSpringToNextBody(), body.GetSpringToPrevBody() } ;
            foreach (var spring in neighborSprings)
            {
                spring.frequency = springFrequency;
                spring.dampingRatio = springDamping;
            }

            if (overrideSpringMiddle)
            {
                var midSpring = body.GetSpringToMiddle();
                midSpring.frequency = springFrequencyMiddle;
                midSpring.distance = springDistanceMiddle;
                midSpring.dampingRatio = springDampingMiddle;
            }

            if (autoPosition)
            {
                int i = body.transform.GetSiblingIndex();
                float anglePerBody = 360f / numBodies;
                PositionBody(anglePerBody, i, this);
            }
        }

        [Button()]
        public void AutoPositionBodies()
        {
            float anglePerBody = 360f / numBodies;
            for (int i = 0; i < numBodies; i++)
            {
                //body.transform.localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (circle.radius + padding);
                PositionBody(anglePerBody, i, this);
            }
        }
        
        public static void PositionBody(float anglePerBody, int i, SoftBody2D softBody)
        {
            float angle = anglePerBody * i;
            var offset = Vector2.right * (softBody.circle.radius + softBody.bodyRadius + softBody.padding);
            offset = Quaternion.Euler(0, 0, angle) * offset;
            softBody.transform.GetChild(i).localPosition = offset;
        }
    }
    
    
    #if UNITY_EDITOR

    [CustomEditor(typeof(SoftBody2D))]
    public class SoftBody2DEditor : OdinEditor
    {
        private SerializedProperty _bodyRadius;
        private SerializedProperty _numBodies;

        protected override void OnEnable()
        {
            _numBodies = serializedObject.FindProperty("numBodies");
            _bodyRadius = serializedObject.FindProperty("bodyRadius");
            base.OnEnable();
        }

        SoftBody2DCollider CreateNewSoftBodyCollider()
        {
            var go = new GameObject("SoftBodyCollider");
            var rb = go.AddComponent<Rigidbody2D>();
            var col = go.AddComponent<CircleCollider2D>();
            col.radius = (target as SoftBody2D).bodyRadius;
            for (int i = 0; i < 3; i++)
            {
                go.AddComponent<SpringJoint2D>();
            }

            var sbc = go.AddComponent<SoftBody2DCollider>();
            return sbc;
        }

        
        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Cannot edit in play mode", MessageType.Warning);
                return;
            }
            var softBody = target as SoftBody2D;
            softBody.INTERNAL_UpdateBodies_EDITOR();
            DrawButtonBar(softBody);
            UpdateBodyCount(softBody);
            base.OnInspectorGUI();
        }

        private void UpdateBodyCount(SoftBody2D softBody)
        {
            //EditorGUI.BeginChangeCheck();
            int prevNumBodies = softBody.transform.childCount;
           // EditorGUILayout.PropertyField(_numBodies);

            int newNumBodies = Mathf.Clamp(_numBodies.intValue, 3, 25);
            if (prevNumBodies < newNumBodies)
            {
                //TODO: Add new bodies
                float anglePerBody = 360f / newNumBodies;

                for (int i = prevNumBodies; i < newNumBodies; i++)
                {
                    var newBody = CreateNewSoftBodyCollider();
                    newBody.transform.SetParent(softBody.transform);
                    PositionBody(anglePerBody, i, softBody);
                }
            }
            else if (prevNumBodies > newNumBodies)
            {
                //TODO: destroy bodies
                for (int i = newNumBodies; i < softBody.transform.childCount; i++)
                {
                    var child = softBody.transform.GetChild(i);
                    Undo.RecordObject(child.gameObject, "Destroy SoftBody2D");
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void DrawButtonBar(SoftBody2D softBody)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reposition Bodies"))
            {
                int cnt = softBody.transform.childCount;
                float anglePerBody = 360f / cnt;
                for (int i = 0; i < cnt; i++)
                {
                    PositionBody(anglePerBody, i, softBody);
                }
            }

            if (GUILayout.Button("Update Bodies"))
            {
                UpdateBodies(softBody);
            }

            GUILayout.EndHorizontal();
        }

        private static void UpdateBodies(SoftBody2D softBody)
        {
            softBody.UpdateBodies();
            for (int i = 0; i < softBody.transform.childCount; i++)
            {
                var childBody = softBody.transform.GetChild(i).GetComponent<SoftBody2DCollider>();
                softBody.UpdateBody(childBody);
            }
        }

        public static void PositionBody(float anglePerBody, int i, SoftBody2D softBody)
        {
            SoftBody2D.PositionBody(anglePerBody, i, softBody);
        }
    }
    
    #endif
    
    
}