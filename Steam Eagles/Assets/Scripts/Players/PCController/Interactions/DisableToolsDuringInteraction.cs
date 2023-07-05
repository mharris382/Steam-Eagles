using CoreLib;
using CoreLib.Interfaces;
using Interactions;
using Tools.BuildTool;
using UnityEngine;

namespace Players.PCController.Interactions
{
    public class DisableToolsDuringInteraction : IPCInteractionStateListener
    {
        private IHideTools _toolSwapController;
        public PC PC
        {
            set
            {
                _toolSwapController = value.PCInstance.character.GetComponent<IHideTools>();
                Debug.Assert(_toolSwapController != null, value.PCInstance.character);
            }
        }
        public void OnPCInteractionStateChanged(bool isInteracting)
        {
            _toolSwapController.ToolsHidden = isInteracting;
        }

        public void OnPCInteractionStateChanged(IInteractable interactable) { }
    }


    public class DisableLabelDuringInteraction : IPCInteractionStateListener
    {
        private IAvailableInteractionLabel _availableInteractionLabel;
        private Component _labelComponent;
        public PC PC
        {
            set
            {
                _availableInteractionLabel =
                    value.PCInstance.character.GetComponentInChildren<IAvailableInteractionLabel>();
                Debug.Assert(_availableInteractionLabel != null, "InteractionLabel is null", value.PCInstance.character);
                _labelComponent = _availableInteractionLabel as Component;
            }
        }
        public void OnPCInteractionStateChanged(bool isInteracting)
        {
            _labelComponent.gameObject.SetActive(!isInteracting);   
        }

        public void OnPCInteractionStateChanged(IInteractable interactable)
        {
            
        }
    }
}