using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class TilesWindow : EditorWindow
    {
        [MenuItem("Tools/Tile Database Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<TilesWindow>();
            window.titleContent = new GUIContent("Tile Database Editor");
            window.Show();
        }

        private void OnGUI()
        {
            //var s = "((SerializeField){1}\s{1})";
        }
    }
}