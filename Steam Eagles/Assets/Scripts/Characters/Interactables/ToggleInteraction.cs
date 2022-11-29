using UnityEngine.Events;

namespace Characters.Interactables
{
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