using System.Collections.Generic;
using CoreLib;
using Zenject;

namespace Interactions.Installers
{
    public class AvailableInteractions : RegistryBase<Interactable>
    {
        public class Factory : PlaceholderFactory<InteractionAgent, AvailableInteractions> { }
        private readonly InteractionAgent _agent;
        private readonly InteractableRegistry _interactableRegistry;
        private readonly HashSet<Interactable> _inRangeInteractables = new HashSet<Interactable>();
        public AvailableInteractions(InteractionAgent agent, InteractableRegistry interactableRegistry)
        {
            _agent = agent;
            _interactableRegistry = interactableRegistry;
        }
        void SetInteractableInRange(Interactable interactable, bool inRange)
        {
            
            if (inRange && !_inRangeInteractables.Contains(interactable))
            {
                _inRangeInteractables.Add(interactable);
                // Debug.Log($"Added Interaction {interactable.name} to {agent.name}");
                _agent.InteractablesInRange.Add(interactable);
                Register(interactable);
            }
            else if(!inRange && _inRangeInteractables.Contains(interactable))
            { 
                //Debug.Log($"Removed Interaction {interactable.name} to {agent.name}");
                _inRangeInteractables.Remove(interactable);
                _agent.InteractablesInRange.Remove(interactable);
                Unregister(interactable);
            }
        }
        public void UpdateInteractions(InteractionAgent agent)
        {
            foreach (var interactable in _interactableRegistry.GetInteractablesInRange(agent))
            {
                SetInteractableInRange( interactable.Item1, interactable.Item2);
            }
        }

        private void NotifyRemoved(Interactable lastInRangeInteractable)
        {
            _agent.InteractablesInRange.Remove(lastInRangeInteractable);
            base.Unregister(lastInRangeInteractable);
        }

        private void NotifyAdded(Interactable interactable)
        {
            _agent.InteractablesInRange.Add(interactable);
            base.Register(interactable);
        }
    }
}