using System.Linq;
using CoreLib.Pickups;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
namespace Items
{
    public class ItemsEditorWindow : OdinMenuEditorWindow
    {
        [UnityEditor.MenuItem("Tools/Items")]
        private static void ShowWindow()
        {
            var window = GetWindow<ItemsEditorWindow>();
            window.titleContent = new UnityEngine.GUIContent("Items");
            window.Show();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            var items = AssetDatabase.FindAssets("t:Item")
                .Select(t => AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(t)));
            var tools = AssetDatabase.FindAssets("t:Tool")
                .Select(t => AssetDatabase.LoadAssetAtPath<Tool>(AssetDatabase.GUIDToAssetPath(t)));
            var recipes = AssetDatabase.FindAssets("t:Recipe")
                .Select(t => AssetDatabase.LoadAssetAtPath<Recipe>(AssetDatabase.GUIDToAssetPath(t)));
            var pickups =AssetDatabase.FindAssets("t:Pickup")
                .Select(t => AssetDatabase.LoadAssetAtPath<Pickup>(AssetDatabase.GUIDToAssetPath(t)));
            foreach (var item in items)
            {
                if (item.icon != null)
                {
                    tree.Add($"Items/{item.name}", item, item.icon);
                }
                else
                {
                    tree.Add($"Items/{item.name}", item);    
                }
            }

            foreach (var tool in tools)
            {
                if (tool.icon != null)
                {
                    tree.Add($"Tools/{tool.name}", tool, tool.icon);
                }
                else
                {
                    tree.Add($"Tools/{tool.name}", tool);    
                }
            }

            foreach (var recipe in recipes)
            {
                if (recipe.icon != null)
                {
                    tree.Add($"Recipes/{recipe.name}", recipe, recipe.icon);
                }
                else
                {
                    tree.Add($"Recipes/{recipe.name}", recipe);    
                }
            }

            foreach (var pickup in pickups)
            {
                tree.Add($"Pickups/{pickup.name}", pickup);    
            }
            return tree;
        }

        
    }
}