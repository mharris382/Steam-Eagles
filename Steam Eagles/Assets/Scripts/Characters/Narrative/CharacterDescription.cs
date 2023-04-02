using System;
using System.Text;
using Characters.Stats;
using CoreLib;
using Sirenix.OdinInspector;

using UnityEngine;

namespace Characters.Narrative
{
    [UnityEngine.CreateAssetMenu(fileName = "New Character", menuName = "Steam Eagles/Characters/New Character",
        order = -1000)]
    public class CharacterDescription : UnityEngine.ScriptableObject
    {
        string EditorDescriptionSummary => BuildDescription().ToString();

        string EditorDescriptionDetailed
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb = BuildDescription();
                
                sb = AppendStartingStats(sb);
                sb = AppendLoadingInfo(sb);
                
                return "";
            }
        }
        
        
        public CharacterComponentReference characterComponentReference;

        [DetailedInfoBox("@EditorDescriptionSummary", "@EditorDescriptionDetailed")]
        [Tooltip("Basic information about the character for use in the UI.")]
        public Basics basicInfo;

        [Tooltip(
            "When we are asked to load a character that we have not met before, we will need to get the stats from somewhere. This is where we will get them from.")]
        public InitialStats startingStats;

        [Tooltip(
            "If this is checked, the character stats will be loaded when the game starts and show up in the character manifest menu. If unchecked that character will need to loaded and then saved at runtime to show up in the manifest menu.")]
        public bool isStartingCharacter;

        [Tooltip("If this is checked, the character component reference must point to a PlayerCharacter component")]
        public bool isPlayableCharacter;

        [Serializable]
        public class Basics
        {
            [LabelWidth(25), LabelText("ui name")]
            [HorizontalGroup("root", width: 300)]
            [VerticalGroup("root/v1")]
            [Tooltip(
                "This name will be used in all text elements displayed to the player, Leave empty to use the name of the asset address. ")]
            public string displayName;

            [HideLabel]
            [PreviewField(ObjectFieldAlignment.Left, Height = 200), HorizontalGroup("root")]
            [Tooltip(
                "The sprite that will be used to represent this character in the UI. UI Element will be hidden if left empty.")]
            public Sprite characterSprite;

            [HideLabel] [VerticalGroup("root/v1"), MultiLineProperty(5)]
            public string description = "description";

            [HideLabel] public UIColorTheme colorTheme;
        }


        [Serializable]
        public class InitialStats
        {
            int GetMaxHealthCap() => MaxStatValues.MAX_HEALTH_CAP;
            int GetMaxEnergyCap() => MaxStatValues.MAX_ENERGY_CAP;
            int GetMaxInventoryCap() => MaxStatValues.MAX_CARRY_WEIGHT;

            [ProgressBar(min: 1, maxGetter: "GetMaxHealthCap")]
            public int health = 10;

            [ProgressBar(min: 1, maxGetter: "GetMaxEnergyCap")]
            public int energy = 10;

            [ProgressBar(min: 1, maxGetter: "GetMaxInventoryCap")]
            public int carryWeight = 10;
        }


        public override string ToString()
        {
            if (Application.isPlaying)
            {
                CharacterManager.Instance.IsCharacterLoaded(this);
            }

            return base.ToString();
        }

        public string AssetName => name;
        public string DisplayName => string.IsNullOrEmpty(basicInfo.displayName) ? AssetName : basicInfo.displayName; 
        public string Description => string.IsNullOrEmpty(basicInfo.description) ? $"TODO: completed character summary for {DisplayName}" : basicInfo.description;
        

        private StringBuilder AddPlayableInfo(StringBuilder sb)
        {
            
            bool isPlayerCharacter = isPlayableCharacter;
            if (isPlayerCharacter)
            {
                sb.AppendLine("Playable Character".InItalics());
            }
            else
            {
                sb.AppendLine("NPC".InItalics());
            }
            return sb;
        }
        private StringBuilder AddFullEditorName(StringBuilder sb)
        {
            bool usesDisplayName = DisplayName == AssetName;
            if (usesDisplayName)
            {
                sb.Append(DisplayName.Bolded());
                sb.Append(' ');
                sb.Append($"({AssetName})\n");
            }
            else
            {
                sb.AppendLine(DisplayName.Bolded());
            }

            return sb;
        }

        private StringBuilder AppendStartingStats(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine(
                $"\nMax Health: {startingStats.health}\n" +
                $"\nMax Energy: {startingStats.energy}\n" +
                $"\nMax Carry Weight: {startingStats.carryWeight}");
            return sb;
        }
       
        private StringBuilder AppendLoadingInfo(StringBuilder sb)
        {
            if (CharacterManager.Instance.IsCharacterLoaded(this))
            {
                sb.AppendLine("Character is spawned!".InItalics().Bolded());
            }
            return sb;
        }
        private StringBuilder BuildDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb = AddFullEditorName(sb);
            sb = AddPlayableInfo(sb);
            sb = AppendStartingStats(sb);
            sb = AppendLoadingInfo(sb);
            return sb;
        }
        
        private StringBuilder AppendDetailedDescription(StringBuilder sb)
        {
            sb.AppendLine();
            
            return sb;
        }

       

    }

    public class CharacterInventory
    {
        
    }
     
    
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