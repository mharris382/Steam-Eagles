using System;
using Players;
using UnityEngine;

namespace Characters.Actions.Abilities
{
    [System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
    public class AbilityManager : MonoBehaviour
    {
        public Player player;
        public GameObject abilityController;


        private void Update()
        {
            if (!HasResources())
            {
                abilityController.gameObject.SetActive(false);
                return;
            }

            var input = player.InputWrapper.PlayerInput;
            if (input.currentActionMap.name != "Gameplay")
            {
                if(abilityController.gameObject.activeSelf)
                    abilityController.gameObject.SetActive(false);
            }
            else
            {
                if(!abilityController.gameObject.activeSelf)
                    abilityController.gameObject.SetActive(true);
            }
            
        }

        bool HasResources()
        {
            if (player == null)
                return false;
            if(abilityController == null)
                return false;
            if (player.InputWrapper == null)
                return false;
            return true;
        }
    }
}