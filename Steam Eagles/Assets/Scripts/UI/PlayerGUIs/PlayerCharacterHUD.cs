using System;
using CoreLib.MyEntities;
using UnityEngine;

namespace UI.PlayerGUIs
{
    public class PlayerCharacterHUD : Window
    {
        protected override bool UsesCloseButton => false;
        protected override bool BlockRaycastsWhenVisible => false;


        public void LinkHUDToEntity(Entity entity, GameObject characterGameObject)
        {
            Debug.Log($"LinkHUDToEntity: \nEntity:{entity}, \nCharacter:{characterGameObject}");
            //throw new NotImplementedException();
        }
    }
}