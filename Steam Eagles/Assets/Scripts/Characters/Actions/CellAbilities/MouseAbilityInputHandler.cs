using System;
using Characters.Actions;
using Characters.Actions.Selectors;
using CoreLib;
using UnityEngine;

namespace Characters
{
    public class MouseAbilityInputHandler : MonoBehaviour
    {
        public Camera camera;
        public SharedCamera sharedCamera;
       public CellSelector selector;
        
        public AbilityController leftMouseButton;
        public AbilityController righttMouseButton;
        public Camera ActiveCam => camera == null ? sharedCamera.Value : camera;

        private void Update()
        {
           
            var wp = ActiveCam.ScreenToWorldPoint(Input.mousePosition);
            leftMouseButton.selector.transform.position = wp;
            righttMouseButton.selector.transform.position = wp;
            leftMouseButton.ShowAbilityPreviewFromMouse(wp);
            if (Input.GetMouseButton(0))
            {
                leftMouseButton.TryAbility(wp);
            }

            if (righttMouseButton == null) return;
            righttMouseButton.ShowAbilityPreviewFromMouse(wp);
            if (Input.GetMouseButton(1))
            {
                righttMouseButton.TryAbility(wp);
            }
        }
    }


    
}