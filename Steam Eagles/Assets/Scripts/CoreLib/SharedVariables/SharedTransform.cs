using System;
using CoreLib;
using UnityEditor;
using UnityEngine;

namespace StateMachine
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Transform", fileName = "SharedTransform", order = 0)]
    public class SharedTransform : SharedVariable<Transform>
    {
        [Tooltip("Optional fallback option used to find desired transform in the scene if it has not bee set.  If tag is empty, it will be ignored.  Primary use case is for when you want to access a transform's value from the editor without playing the scene")]
        public string lookupTag;
        public override Transform Value
        {
            get
            {
                if (base.Value == null && !string.IsNullOrEmpty(lookupTag))
                {
                    var foundGo = GameObject.FindWithTag(lookupTag);
                    if (!foundGo)
                    {
                        Debug.LogWarning($"No object tagged with {lookupTag} found in scene!", this);
                        return null;
                    }
                    base.Value = foundGo.transform;
                }
                return base.Value;
            }
            set => base.Value = value;
        }

        
        
        /// <summary>
        /// Position property for transform 
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (!HasValue) throw new NullReferenceException();
                return Value.position;
            }
            set
            {
                if (!HasValue) return;
                this.Value.position = value;
            }
        }
        /// <summary>
        /// local Position property for transform 
        /// </summary>
        public Vector3 LocalPosition
        {
            get
            {
                if (!HasValue) throw new NullReferenceException();
                return Value.localPosition;
            }
            set
            {
                if (!HasValue) return;
                this.Value.localPosition = value;
            }
        }

        public void SetPositionIfHasValue(Vector3 position)
        {
            if (HasValue)
                Position = position;
        }
    }
    
        
#if UNITY_EDITOR
    [CustomEditor(typeof(SharedTransform))]
    public class SharedTransformEditor : SharedVariableEditor<Transform, SharedTransform>
    {
        protected override void OnVariableGUI(SharedTransform t)
        {
            if (!Application.isPlaying)
            {
                if (t.HasValue == false && !string.IsNullOrEmpty(t.lookupTag))
                {
                    if (GUILayout.Button($"Search For Transform with tag: {t.lookupTag}"))
                    {
                        var go = GameObject.FindGameObjectWithTag(t.lookupTag);
                        if (go != null)
                        {
                            t.Value = go.transform;
                        }
                    }
                }
            }
            base.OnVariableGUI(t);
        }
    }
#endif
    
}