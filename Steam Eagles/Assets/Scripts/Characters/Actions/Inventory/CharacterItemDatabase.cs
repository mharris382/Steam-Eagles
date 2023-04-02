using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Characters.Actions.Inventory
{
    [System.Obsolete("all inventory code is now the responsibility of the items assembly")]
    public class CharacterItemDatabase : ScriptableObject
    {
        public List<ItemData> itemData;
    }
    
    
  
}