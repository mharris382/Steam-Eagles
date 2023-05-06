using CoreLib;
using Tools.BuildTool;
using UnityEngine;

namespace Players.PCController.Interactions
{
    public class DisableToolsDuringInteraction : IPCInteractionStateListener
    {
        private ToolSwapController _toolSwapController;
        public PC PC
        {
            set
            {
                _toolSwapController = value.PCInstance.character.GetComponent<ToolSwapController>();
                Debug.Assert(_toolSwapController != null, value.PCInstance.character);
            }
        }
        public void OnPCInteractionStateChanged(bool isInteracting)
        {
            _toolSwapController.ToolsHidden = isInteracting;
        }

        public void OnPCInteractionStateChanged(IInteractable interactable) { }
    }
}