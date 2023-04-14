using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Interactions
{
    /// <summary>
    /// represents entity that is able to perform interactions with registered interactions entities
    /// </summary>
    public abstract class InteractionAgent : MonoBehaviour,  IEntityID
    {
        public abstract string GetEntityGUID();
        
                
        public bool InteractPressed { get; set; }
        public bool InteractHeld { get; set; }

        private ReactiveProperty<Interactable> _selectedInteractable = new ReactiveProperty<Interactable>();
        private ReactiveCollection<Interactable> _interactablesInRange = new ReactiveCollection<Interactable>();
        private BoolReactiveProperty _isInteracting = new BoolReactiveProperty(false);
        
        public IReadOnlyReactiveProperty<Interactable> SelectedInteractable => _selectedInteractable ??= new ReactiveProperty<Interactable>();
        public IReadOnlyReactiveProperty<bool> IsInteracting => _isInteracting ??= new BoolReactiveProperty(false);
        public bool HasInteractableInRange => _interactablesInRange.Count > 0;
        
        public ReactiveCollection<Interactable> InteractablesInRange => _interactablesInRange ??= new ReactiveCollection<Interactable>();
        private void Awake()
        {
            _interactablesInRange.ObserveCountChanged()
                .Where(t => t > 0)
                .Subscribe(_ => _selectedInteractable.Value = _interactablesInRange[0])
                .AddTo(this);
            
            _interactablesInRange.ObserveRemove()
                .Where(t => t.Value == SelectedInteractable.Value)
                .Subscribe(_ => _selectedInteractable.Value = _interactablesInRange.Count > 0 ? _interactablesInRange[0] : null)
                .AddTo(this);
            _interactablesInRange.ObserveAdd().Subscribe(t => Debug.Log($"Added {t.Value} to {name}")).AddTo(this);
            InteractionManager.Instance.RegisterInteractionAgent(this);
            _isInteracting.Value = false;
            _isInteracting.Subscribe(isInteracting => Debug.Log($"{name} has {(isInteracting ? "started" : "finished")} interacting with {(SelectedInteractable.Value != null ? SelectedInteractable.Value.name : null)}")).AddTo(this);
        }

        private void OnDestroy()
        {
            InteractionManager.Instance.UnregisterInteractionAgent(this);
        }

        Coroutine _interactionStartCoroutine;


        private void Update()
        {
            if(_interactionStartCoroutine != null) return;
            if(IsInteracting.Value) return;
            if(!HasInteractableInRange) return;
            
            SelectBestInteractable();
            
            if (HasInteractableInRange && InteractPressed)
            {
                _interactionStartCoroutine = StartCoroutine(InteractionStart(SelectedInteractable.Value));
            }
        }

        private void SelectBestInteractable()
        {
            //TODO: choose best interactable from availble and set it as selected
        }

        IEnumerator InteractionStart(Interactable interactable)
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                _isInteracting.Value = true;
                var result = await interactable.Interact(this);
                if (!result) NotifyFailure(interactable);
                _isInteracting.Value = false;
            });
             _interactionStartCoroutine = null;
        }

        /// <summary>
        /// this is so that we can notify the player that they failed to interact with something
        /// </summary>
        /// <param name="interactable"></param>
        /// <exception cref="NotImplementedException"></exception>
       private void NotifyFailure(Interactable interactable)
       {
           Debug.Log($"{name} Failed to interact with {interactable}");
           throw new NotImplementedException();
       }
    }
}