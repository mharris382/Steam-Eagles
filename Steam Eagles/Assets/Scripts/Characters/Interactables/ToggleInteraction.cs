using UnityEngine.Events;

namespace Characters.Interactables
{
    [System.Obsolete("Interaction System will be replaced with a generic version to be used by both NPC (AI) and PC (players)")]
    public class ToggleInteraction : InteractableObject
    {
        public bool startOn;
        public UnityEvent onToggledOn;
        public UnityEvent onToggledOff;

        private bool _on;

        public override void OnInteractionTriggered(InteractionController actor)
        {
            _on = !_on;
            if (_on)
            {
                onToggledOn?.Invoke();
            }
            else
            {
                onToggledOff?.Invoke();
            }
            base.OnInteractionTriggered(actor);
        }
    }
}