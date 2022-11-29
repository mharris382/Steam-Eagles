using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Characters.Actions.Inventory
{
    [CreateAssetMenu(fileName = "New Inventory Item", menuName = "Steam Eagles/Character Item Database")]
    public class CharacterItemDatabase : ScriptableObject
    {
        public List<ItemData> itemData;
    }
    
    
  
}