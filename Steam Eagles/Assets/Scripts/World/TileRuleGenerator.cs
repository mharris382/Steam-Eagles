using UnityEngine;
using UnityEditor;
namespace World
{
    public class TileRuleGenerator : UnityEditor.EditorWindow
    {
        [UnityEditor.MenuItem("Tools/Tile Rule Generator")]
        private static void ShowWindow()
        {
            var window = GetWindow<TileRuleGenerator>();
            window.titleContent = new UnityEngine.GUIContent("Tile Rule Generator");
            window.Show();
        }

        private void OnGUI()
        {
            
        }
    }
}