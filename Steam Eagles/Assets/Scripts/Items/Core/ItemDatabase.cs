using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Pickups;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Items.Core
{
    public class ItemDatabase : MonoBehaviour
    {
        [SerializeField]
        private List<string> itemNames = new List<string>();
        [System.Serializable]
        public class ItemEntry
        {
            public string name;
            
            public ItemBase loadedItem;
            public Pickup loadedPickup;
            
            public ItemEntry(MonoBehaviour owner, string name)
            {
                this.name = name;
                owner.StartCoroutine(LoadEntry());
            }
            
            public bool IsFullyLoaded => loadedItem != null && loadedPickup != null;

            IEnumerator LoadEntry()
            {
                Debug.Log($"Loading {name.Bolded()}...");
                yield return this.name.LoadItem(t => loadedItem = t);
                yield return this.name.LoadPickup(t => loadedPickup = t);
                
                Debug.Assert(loadedItem != null, $"Failed to load item {name}");
                Debug.Assert(loadedPickup != null, $"Failed to load pickup {name}");

                Debug.Log(IsFullyLoaded
                    ? $"Successfully loaded {name.Bolded()}!".ColoredGreen()
                    : $"Failed to load {name.Bolded()}!".ColoredRed());
            }
        }
    }
    
}
