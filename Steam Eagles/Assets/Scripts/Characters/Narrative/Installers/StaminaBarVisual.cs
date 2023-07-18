using System;
using CoreLib;
using CoreLib.MyEntities;
using UnityEngine;
using Zenject;

namespace Characters.Narrative.Installers
{
    public class StaminaBarVisual : MonoBehaviour
    {
        public SpriteRenderer staminaBar;
        public SpriteRenderer staminaBarBackground;

        public float alpha
        {
            get => staminaBar.color.a;
            set
            {
                staminaBar.color = staminaBar.color.SetAlpha(value);
                staminaBarBackground.color = staminaBar.color.SetAlpha(value);
            }
        }

        private Vector2 _fullSize;



        [Inject]
        private void InjectMe(Entity entity)
        {
            
        }
        
        private void Awake()
        {
            _fullSize = staminaBar.size;
        }
        
        
    }
}