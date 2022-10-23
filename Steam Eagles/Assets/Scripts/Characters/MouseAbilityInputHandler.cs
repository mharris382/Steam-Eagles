using System;
using Characters.Actions;
using CoreLib;
using UnityEngine;

namespace Characters
{
    public class MouseAbilityInputHandler : MonoBehaviour
    {
        public Camera camera;
        public SharedCamera sharedCamera;
        public CellSelector selector;
        
        public Ability leftMouseButton;

        public Camera ActiveCam => camera == null ? sharedCamera.Value : camera;

        private void Update()
        {
           
            var wp = ActiveCam.ScreenToWorldPoint(Input.mousePosition);
            selector.transform.position = wp;
            leftMouseButton.ShowAbilityPreviewFromMouse(wp);
            if (Input.GetMouseButton(0))
            {
                leftMouseButton.TryAbility(wp);
            }
        }
    }
}