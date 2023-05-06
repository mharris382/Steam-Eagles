using CoreLib;

namespace Players.PCController.Interactions
{
    public interface IPCInteractionStateListener
    {
        public PC PC { set; }
        void OnPCInteractionStateChanged(bool isInteracting);
        void OnPCInteractionStateChanged(IInteractable interactable);
    }
}