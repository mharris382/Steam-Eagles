using System.Linq;
using CoreLib.Pickups;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEngine;

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
            
            ItemOverview.Instance.UpdateItems(); 
            tree.Add("Items",ItemOverview.Instance);
            foreach (var item in items)
            {
                AddItemToTree(item, tree);
            }
            foreach (var tool in tools)
            {
                AddItemToTree(tool, tree, "Tools");
            }
            AddRecipesToMenu(tree);
            AddPickupsToMenu(tree);
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            var selected = this.MenuTree.Selection.FirstOrDefault();
            var toolbarHeight = this.MenuTree.Config.SearchToolbarHeight;
            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
            {
                if (selected != null)
                {
                    GUILayout.Label(selected.Name);
                }

                if (SirenixEditorGUI.ToolbarButton(new GUIContent("Create Item")))
                {
                    ScriptableObjectCreator.ShowDialog<ItemBase>("Data/Items", obj =>
                    {
                        obj.itemName = obj.name;
                        TrySelectMenuItemWithObject(obj);
                    });
                }
                if (SirenixEditorGUI.ToolbarButton(new GUIContent("New Recipe")))
                {
                    ScriptableObjectCreator.ShowDialog<Recipe>("Data/Recipes", obj =>
                    {
                        TrySelectMenuItemWithObject(obj);
                    });
                }
                if(SirenixEditorGUI.ToolbarButton(new GUIContent("New Pickup")))
                {
                    ScriptableObjectCreator.ShowDialog<Pickup>("Data/Pickups", obj =>
                    {
                        var n = obj.name;
                        var underscore= n.IndexOf('_');
                        if (underscore > 0)
                        {
                            obj.key = name.Substring(0, underscore);
                        }
                        TrySelectMenuItemWithObject(obj);
                    });
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
            base.OnBeginDrawEditors();
        }

        private static void AddPickupsToMenu(OdinMenuTree tree)
        {
            var pickups = AssetDatabase.FindAssets("t:Pickup")
                .Select(t => AssetDatabase.LoadAssetAtPath<Pickup>(AssetDatabase.GUIDToAssetPath(t)));
            foreach (var pickup in pickups)
            {
                if (pickup.DefaultSprite != null)
                {
                    tree.Add($"Pickups/{pickup.name}", pickup, pickup.DefaultSprite);
                }
                else
                {
                    tree.Add($"Pickups/{pickup.name}", pickup);
                }

            }
        }

        private static void AddRecipesToMenu(OdinMenuTree tree)
        {
            var recipes = AssetDatabase.FindAssets("t:Recipe")
                .Select(t => AssetDatabase.LoadAssetAtPath<Recipe>(AssetDatabase.GUIDToAssetPath(t)));
            foreach (var recipe in recipes)
            {
                AddRecipeToTree(recipe, tree);
            }
        }

        private static void AddItemToTree(Item item, OdinMenuTree tree)
        {
            string group = "Items";
            AddItemToTree(item, tree, group);
        }
        
        private static void AddRecipeToTree(Recipe recipe, OdinMenuTree tree)
        {
            string group = "Recipes";
            if (recipe.icon != null)
            {
                tree.Add($"Recipes/{recipe.name}", recipe, recipe.icon);
            }
            else
            {
                tree.Add($"Recipes/{recipe.name}", recipe);    
            }
        }
        private static void AddItemToTree(ItemBase item, OdinMenuTree tree, string group)
        {
            bool isDepricated = item.name.StartsWith("_DEP");
            if (isDepricated)
            {
                group = $"{group}/Depricated";
            }
            if (item.icon != null)
            {
                tree.Add($"{group}/{item.itemName}", item, item.icon);
            }
            else
            {
                tree.Add($"Items/{item.itemName}", item);
            }
        }
    }
}