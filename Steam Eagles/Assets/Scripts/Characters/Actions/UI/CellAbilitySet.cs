using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Actions.UI
{
    public class CellAbilitySet : MonoBehaviour, ICellAbilitySet
    {
        [SerializeField]
        private Set[] sets;
        [SerializeField,Required] private SharedInt selectedAbilitySetIndex;
        
        [Serializable]
       private class Set
       {
          [Required] public CellAbility primaryAbility;
          [Required] public CellAbility secondaryAbility;
       }

        public CellAbility PrimaryAbility  => sets[selectedAbilitySetIndex.Value].primaryAbility;
        public CellAbility SecondaryAbility => sets[selectedAbilitySetIndex.Value].secondaryAbility;
        public CellAbility GetAbility(int index)
        {
            switch (index)
            {
                case 0:
                    return sets[selectedAbilitySetIndex.Value].primaryAbility;
                case 1:
                    return sets[selectedAbilitySetIndex.Value].secondaryAbility;
                default:
                    return null;
            }
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Alpha1))
                selectedAbilitySetIndex.Value = 0;
            if(Input.GetKeyDown(KeyCode.Alpha2))
                selectedAbilitySetIndex.Value = 1;
        }

        public IEnumerator<CellAbility> GetEnumerator()
        {
            yield return GetAbility(0);
            yield return GetAbility(1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}