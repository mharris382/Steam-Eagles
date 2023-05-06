using System;
using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;

namespace Interactions
{
    public class InteractionManager : Singleton<InteractionManager>
    {
        public override bool DestroyOnLoad => true;

        List<InteractionAgent> _interactionAgents = new List<InteractionAgent>();
        List<Interactable> _interactables = new List<Interactable>();
        Dictionary<Interactable, float> _interactableDistances = new Dictionary<Interactable, float>();
        Dictionary<InteractionAgent, HashSet<Interactable>> _interactableInRange = new Dictionary<InteractionAgent, HashSet<Interactable>>();
        

        public void RegisterInteractionAgent(InteractionAgent interactionAgent)
        {
            if(_interactionAgents.Contains(interactionAgent))
                return;
            _interactionAgents.Add(interactionAgent);
            _interactableInRange.Add(interactionAgent , new HashSet<Interactable>());
        }

        public void UnregisterInteractionAgent(InteractionAgent interactionAgent)
        {
            _interactionAgents.Remove(interactionAgent);
            _interactableInRange.Remove(interactionAgent);
        }

        public void RegisterInteractable(Interactable interactable)
        {
            if(_interactables.Contains(interactable))
                return;
            _interactables.Add(interactable);
            _interactableDistances.Add(interactable, interactable.range * interactable.range);
        }

        public void UnregisterInteractable(Interactable interactable)
        {
            _interactables.Remove(interactable);
            _interactableDistances.Remove(interactable);
        }


        private void Update()
        {
            foreach (var agent in _interactionAgents) 
                UpdateInteractions(agent);
        }

        void UpdateInteractions(InteractionAgent agent)
        {
            foreach (var interactable in _interactables)
            {
                if(interactable == null) continue;
                var dist =(agent.transform.position - interactable.transform.position).sqrMagnitude;
                bool inRange = dist <= _interactableDistances[interactable];
                SetInteractableInRange(agent, interactable, inRange);
            }
        }

        void SetInteractableInRange(InteractionAgent agent, Interactable interactable, bool inRange)
        {
            if (inRange && !_interactableInRange[agent].Contains(interactable))
            {
                _interactableInRange[agent].Add(interactable);
                Debug.Log($"Added Interaction {interactable.name} to {agent.name}");
                agent.InteractablesInRange.Add(interactable);
            }
            else if(!inRange && _interactableInRange[agent].Contains(interactable))
            { 
                Debug.Log($"Removed Interaction {interactable.name} to {agent.name}");
                _interactableInRange[agent].Remove(interactable);
                agent.InteractablesInRange.Remove(interactable);
            }
        }
    }
}