using CoreLib;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Characters
{
    [RequireComponent(typeof(CharacterInputState))]
    public class InputLogger : MonoBehaviour
    {
        private CharacterInputState _inputState;
        
        private void Awake()
        {
            this._inputState = GetComponent<CharacterInputState>();
            
            SubscribeToInput(_inputState.onInteract, "Interact".Bolded());
            SubscribeToInput(_inputState.onPickup, "Pickup".Bolded());
            SubscribeToInput(_inputState.onJump, "Jump".Bolded());
        }
    

        void SubscribeToInput(UnityEvent<InputAction.CallbackContext> inputEvent, string eventLabel)
        {
            inputEvent.AsObservable().TakeUntilDisable(this).Debug().Subscribe(_ => Debug.Log($"Input {eventLabel} Occured!"));
        }
        
        
    }
}