using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Stats
{
    [CreateAssetMenu(fileName = "New Stat", menuName = "Steam Eagles/Characters/New Stat", order = 0)]
    public class StatDefinition : ScriptableObject
    {
        [SerializeField]
        [ListDrawerSettings(Expanded = true, ShowIndexLabels = true, ListElementLabelName = "rankName")]
        private List<StatRank> statRanks = new List<StatRank>();
        
        
        [Serializable]
        public class StatRank
        {
            public string rankName = "New Stat Rank";
            [PreviewField]
            public Sprite rankSprite;
        }
    }
}