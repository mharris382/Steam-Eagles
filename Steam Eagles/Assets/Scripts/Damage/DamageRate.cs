using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Damage
{
    [InlineProperty]
    [System.Serializable]
    public class DamageRate
    {
        [Min(1)]
        [HorizontalGroup("h1", width:0.5f),HideLabel,SuffixLabel("damage every")]
        [SerializeField] private int damage = 1;
        
        [Min(1)]
        [HorizontalGroup("h1", width:0.5f),HideLabel, SuffixLabel("Turns")]
        [SerializeField] private int turns = 3;

        public DamageRate(int damage, int turns)
        {
            this.damage = damage;
            this.turns = turns;
        }


        
        /// <summary>
        /// note this doesn't account for the number of checks per turn (assumes only 1 check per turn)
        /// </summary>
        /// <returns></returns>
        public float GetProbability() => damage / (float)turns;


        public float GetProbability(int checksPerTurn)
        {
            float damagePerTurn = GetProbability();
            float damagePerCheck = damagePerTurn / checksPerTurn;
            return damagePerCheck;
        }

        public static DamageRate Lerp(DamageRate rateA, DamageRate rateB, float percent)
        {
            float damage = Mathf.Lerp(rateA.damage, rateB.damage, percent);
            float turns = Mathf.Lerp(rateA.turns, rateB.turns, percent);
            int damageI = Mathf.CeilToInt(damage);
            int turnsI = Mathf.CeilToInt(turns);
            return new DamageRate(damageI, turnsI);
        }
        
        public static DamageRate Lerp(DamageRate rateA, DamageRate rateB, float percent, float p2)
        {
            float probabilityA = rateA.GetProbability();
            float probabilityB = rateB.GetProbability();
            float probabilityC = Mathf.Lerp(probabilityA, probabilityB, percent);
            //there are two possible damage rates that can produce probability C
            //rate 1 is calculated using the minimum number of turns (all damage done on single turn) 
            throw new NotImplementedException();
            //rate 2 is calculated using the maximum number of turns (damage spread out over most possible turns)
            
            float damage = Mathf.Lerp(rateA.damage, rateB.damage, percent);
            float turns = Mathf.Lerp(rateA.turns, rateB.turns, percent);
            int damageI = Mathf.CeilToInt(damage);
            int turnsI = Mathf.CeilToInt(turns);
            return new DamageRate(damageI, turnsI);
        }
    }
}