using System;
using UnityEditor;
using UnityEngine;

namespace Characters
{
    public class ValveParticles : MonoBehaviour
    {
        public class ParticleState
        {
            [Tooltip("Particle System is played when valve is in desired state.  Certain parame")]
            public ParticleSystem stateParticles;
            
            [Tooltip("Triggers this particle system when the valve enters this state.")]
            public ParticleSystem enterStateParticles;
            
            [Tooltip("Triggers this particle system when the valve enters this state.")]
            public ParticleSystem exitStateParticles;
        }

        [Serializable]
        public class ParticleStateParam
        {
            public bool enabled;
            public ParticleSystem.MinMaxCurve rateOverValvePercent;
        }
#if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(ParticleStateParam))]
        public class ParticleStateParamDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                var toggleRect = new Rect(position);
                toggleRect.width = 20;
                position.min = toggleRect.max;
                position.width -= toggleRect.width;
                
                var enabledProp =  property.FindPropertyRelative("enabled");
                EditorGUI.PropertyField(toggleRect, enabledProp, GUIContent.none);
                
                var controlRect = EditorGUI.PrefixLabel(position, label);
                EditorGUI.indentLevel = 0;
                EditorGUI.PropertyField(controlRect, property.FindPropertyRelative("rateOverValvePercent"), GUIContent.none);
            }
        }
#endif
    }
}