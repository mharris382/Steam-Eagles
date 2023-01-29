using System;
using Players;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Characters.Actions.Abilities
{
    public abstract class AbilityControllerBase : MonoBehaviour
    {
        [Required]
        [SerializeField] private Player player;
        public void OnEnable()
        {
            ShowUI();
        }
        
        public void OnDisable()
        {
            HideUI();
        }

        public abstract void ShowUI();
        
        public abstract void HideUI();
        
    }
}