using System;
using Players;
using UniRx;
using UnityEngine;
using Observable = UnityEngine.InputSystem.Utilities.Observable;

namespace Characters.Interactables
{
    
    /// <summary>
    /// handles the core logic of interactions
    /// </summary>
    public class InteractionController : MonoBehaviour
    {
        public string tag;
        private CharacterState _state;
        private Transform _target;
        public float interactionRadius = 4;
        public Player player;
        
        
        public Transform Target
        {
            set
            {
                _target = value;
                if(_target)
                    _state = _target.GetComponent<CharacterState>();
            }
            get => _target ? _target : transform;
        }

        public CharacterState State
        {
            get => _state ? _state : (_state = Target.GetComponentInParent<CharacterState>());
        }
        
        public Vector2 Position => Target.position;

        internal ReactiveProperty<InteractableObject> _currentAvailableInteractable = new ReactiveProperty<InteractableObject>();
        IDisposable _currentAvailableInteractableDisposable;
        Subject<string> _emptyDescription = new Subject<string>();
        public InteractableObject CurrentAvailableInteractable  
        {
            get => _currentAvailableInteractable.Value;
            set
            {
                if (value != _currentAvailableInteractable.Value)
                {
                    if(_currentAvailableInteractableDisposable != null)
                        _currentAvailableInteractableDisposable.Dispose();
                    _currentAvailableInteractable.Value = value;
                    OnCurrentInteractableChanged(value);
                    if (value != null)
                    {
                        _currentAvailableInteractableDisposable =
                            value.OnDescriptionChange.Subscribe(OnInteractableDescriptionChanged);
                    }
                    else
                    {
                        _emptyDescription.OnNext("");    
                    }
                }
            }
        }

        

        void OnInteractableDescriptionChanged(string description)
        {
        }


        private void Awake()
        {
            InteractionManager.Instance.controllers.Add(this);
            MessageBroker.Default.Receive<InteractActionEvent>().Where(t => t.tag == tag)
                .Where(t => t.context.performed)
                .Subscribe(OnInteractionEvent)
                .AddTo(this);

        }
        
        private void OnCurrentInteractableChanged(InteractableObject value)
        {
            
        }

        private void OnInteractionEvent(InteractActionEvent interactActionEvent)
        {
            Debug.Log("OnInteractEvent");
            if (CurrentAvailableInteractable != null)
            {
                CurrentAvailableInteractable.OnInteractionTriggered(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var color = Color.Lerp(Color.magenta, Color.white, 0.45f);
            color.a = 0.5f;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }

    public class AvailableInteraction
    {
        public string interactionLabel;
        public string inputLabel;
        
        public Transform actor;
    }
}

