using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Stats
{
    public static class MaxStatValues
    {
        public const int MAX_HEALTH_CAP = 100;
        public const int MAX_ENERGY_CAP = 100;
        public const int MAX_INVENTORY_CAP = 30;
    }
    public static class Stats
    {


        public static void LoadStats(string path)
        {
            
        }
    }

    

    [Serializable]
    public class AllStats
    {
        
    }

    [Serializable]
    public class CharacterStats
    {
        public string characterName;
        public CrewMemberStats crewMemberStats;
        public CharacterStats(string characterName)
        {
            this.characterName = characterName;
        }
    }

    [Serializable]
    public class RangedStat
    {
        [SerializeField, HideInInspector]
        private Vector2Int _stat;

        

        [ShowInInspector]
        public int Value
        {
            get => _stat.x;
            set => _stat.x = Mathf.Clamp(value, 0, MaxValue);
        }
        
        
        [ShowInInspector]
        public int MaxValue
        {
            get => _stat.y;
            set
            {
                _stat.y = Mathf.Max(0, value);
                Value = Value;
            }
        }
        
        public RangedStat(int value, int maxValue)
        {
            _stat = new Vector2Int(value, maxValue);
        }
        
    }

    [Serializable]
    public class RegenStat : RangedStat
    {
        [SerializeField]
        private float regenRate;
        
        public RegenStat(int value, int maxValue, float regenRate) : base(value, maxValue)
        {
            this.regenRate = regenRate;
        }
        
        public void Regen(float deltaTime)
        {
            float regen = regenRate * deltaTime;
            Value += Mathf.RoundToInt(regen);
        }
    }
    
    [Serializable]
    public class PhysicalStats
    {
        public PhysicalStats()
        {
            health = new RangedStat(10, 10);
            energy = new RegenStat(100, 100, 1);
            inventorySize = 7;
        }
        public PhysicalStats(int maxHealth=10, int maxEnergy=100, float energyRegenRate=.5f, int inventorySize=7)
        {
            health = new RangedStat(maxHealth, maxHealth);
            energy = new RegenStat(maxEnergy, maxEnergy, energyRegenRate);
            this.inventorySize = inventorySize;
        }
        
        [SerializeField] private RangedStat health;
        [SerializeField] private RegenStat energy;
        
        public int inventorySize = 7;
    }
    
    [Serializable]
    public class CrewMemberStats
    {
        public int pilotRank;
        public int engineerRank;
        public int officerRank;
    }
    
    
    
}