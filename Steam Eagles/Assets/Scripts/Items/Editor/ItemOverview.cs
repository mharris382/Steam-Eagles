using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Items
{
    [GlobalConfig("Data/Items")]
    public class ItemOverview : GlobalConfig<ItemOverview>
    {
        [ReadOnly]
        [ListDrawerSettings(Expanded = true)]
        public ItemBase[] AllItems;

        public void UpdateItems()
        {
            this.AllItems = AssetDatabase.FindAssets("t:ItemBase")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ItemBase>(AssetDatabase.GUIDToAssetPath(guid)))
                .ToArray();
        }
    }
}