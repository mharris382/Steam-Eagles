using System;
using Characters.Actions;
using CoreLib;
using UnityEngine;

namespace Characters
{
    public class MouseAbilityInputHandler : MonoBehaviour
    {
        public SharedCamera sharedCamera;
        public CellSelector selector;
        
        public Ability leftMouseButton;
       

        private void Update()
        {
            if (!sharedCamera.HasValue)
            {
                Debug.LogWarning("Missing Shared Camera Value!", this);
                return;
            }
            var wp = sharedCamera.Value.ScreenToWorldPoint(Input.mousePosition);
            selector.transform.position = wp;
            leftMouseButton.ShowAbilityPreviewFromMouse(wp);
            if (Input.GetMouseButton(0))
            {
                leftMouseButton.TryAbility(wp);
            }
        }
    }
}