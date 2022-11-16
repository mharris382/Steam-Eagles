#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GasSim
{
    [CustomEditor(typeof(GasSimParticleSystem))]
    public class GasSimParticleSystemEditor : Editor
    {
        private bool expanded = false;

        public override void OnInspectorGUI()
        {
            var gasSim = target as GasSimParticleSystem;
            var pressureColor = gasSim.pressureColor;
            if (EditorGUILayout.Foldout(expanded, "Pressure to Color"))
            {
                GUILayout.BeginVertical();
                EditorGUI.indentLevel++;
                for (int i = 0; i < 16; i++)
                {
                    GUILayout.BeginHorizontal();

                    const float spacePressureIn = 32f; const float spaceColor = 0.4f; const float spacePressureOut = 0.2f;
                    int pressureIn = i;
                    EditorGUILayout.LabelField(pressureIn.ToString(), GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            
                    var color = pressureColor.PressureToColor(i);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ColorField(color, GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                    EditorGUI.EndDisabledGroup();
                    int pressureOut = pressureColor.ColorToPressure(color);
                    EditorGUILayout.LabelField(pressureOut.ToString(), GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);
        
        
            if (Application.isPlaying)
            {
               
            }
       
        
            base.OnInspectorGUI();
        }
    }
}

#endif