

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Stats
{
    [UnityEngine.CreateAssetMenu(fileName = "New Character", menuName = "Steam Eagles/Characters/New Character", order = -1000)]
    public class CharacterDescription : UnityEngine.ScriptableObject
    {
        [Tooltip("Basic information about the character for use in the UI.")]
        public Basics basicInfo;
        
        [Tooltip("When we are asked to load a character that we have not met before, we will need to get the stats from somewhere. This is where we will get them from.")]
        public InitialStats startingStats;
        
        [Tooltip("If this is checked, the character stats will be loaded when the game starts and show up in the character manifest menu. If unchecked that character will need to loaded and then saved at runtime to show up in the manifest menu.")]
        public bool isStartingCharacter;
        
        
        [Serializable]
        public class Basics
        {
            [LabelWidth(25), LabelText("ui name")]
            [HorizontalGroup("root", width:300)]
            [VerticalGroup("root/v1")]
            [Tooltip("This name will be used in all text elements displayed to the player, Leave empty to use the name of the asset address. ")]
            public string displayName;
            
            [PreviewField(ObjectFieldAlignment.Left, Height = 200), HorizontalGroup("root")]
            [Tooltip("The sprite that will be used to represent this character in the UI. UI Element will be hidden if left empty.")]
            public Sprite characterSprite;
            
            [HideLabel]
            [VerticalGroup("root/v1"),MultiLineProperty(5)]
            public string description = "description";

            public UIColorTheme colorTheme;
        }
        
        
        [Serializable]
        public class InitialStats
        {
            int GetMaxHealthCap() => MaxStatValues.MAX_HEALTH_CAP;
            int GetMaxEnergyCap() => MaxStatValues.MAX_ENERGY_CAP;
            int GetMaxInventoryCap() => MaxStatValues.MAX_INVENTORY_CAP;
            
            [ProgressBar(min:1, maxGetter:"GetMaxHealthCap")]
            public int health = 10;
            
            [ProgressBar(min:1, maxGetter:"GetMaxEnergyCap")]
            public int energy = 10;
            
            [ProgressBar(min:1, maxGetter:"GetMaxInventoryCap")]
            public int inventorySpace = 10;
        }
        
        
        public override string ToString()
        {
            if (Application.isPlaying)
            {
                CharacterManager.Instance.IsCharacterLoaded(this);
            }
            return base.ToString();
        }
    }


    public class 
    
    [InlineProperty]
    [System.Serializable]
    public class UIColorTheme
    {
        
        public bool hasColorTheme;
        [Toggle("hasColorTheme")]
        [ColorPalette]
        public Color color;
    }
}