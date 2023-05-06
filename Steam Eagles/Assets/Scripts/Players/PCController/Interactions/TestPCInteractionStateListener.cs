using CoreLib;
using UnityEngine;

namespace Players.PCController.Interactions
{
    public class TestPCInteractionStateListener : IPCInteractionStateListener
    {
        public PC PC
        {
            get;
            set;
        }

        public void OnPCInteractionStateChanged(bool isInteracting)
        {
            Debug.Log($"{PC.PlayerNumber} is interacting: {isInteracting}");
        }

        public void OnPCInteractionStateChanged(IInteractable interactable)
        {
            if (interactable != null)
            {
                Debug.Log($"{PC.PlayerNumber} is interacting with {interactable.name}");
            }
        }
    }
    public class TestPCInteractionStateListener2 : TestPCInteractionStateListener
    {
        
    }
}