#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using UnityEngine;

namespace PhysicsFun
{
    public abstract class WallFaderBase : MonoBehaviour
    {
        public abstract void SetWallAlpha(float alpha);

        public abstract float GetWallAlpha();
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(WallFaderBase), true)]
    public class WallFaderBaseEditor : OdinEditor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            var fader = target as WallFaderBase;
            if (GUILayout.Button("Hide"))
            {
                fader.SetWallAlpha(0f);
            }
            if (GUILayout.Button("25%"))
            {
                fader.SetWallAlpha(0.25f);
            }
            else if(GUILayout.Button("50%"))
            {
                fader.SetWallAlpha(.5f);
            }
            else if(GUILayout.Button("100%"))
            {
                fader.SetWallAlpha(1f);
            }
            EditorGUILayout.EndHorizontal();
            base.OnInspectorGUI();
        }
    }
#endif

}