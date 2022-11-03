using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class BlockDatabaseWindow : EditorWindow
    {
        [MenuItem("Steam Eagles/Block Database Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<BlockDatabaseWindow>();
            window.titleContent = new GUIContent("Block Database Editor");
            window.Show();
        }

   
    }
}