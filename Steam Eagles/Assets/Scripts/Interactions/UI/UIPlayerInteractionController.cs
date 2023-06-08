using System;
using System.Collections;
using Game;
using Players.Shared;
using UI.PlayerGUIs;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Interactions.UI
{
    [RequireComponent(typeof(PlayerCharacterGUIController))]
    public class UIPlayerInteractionController : MonoBehaviour
    {
        public bool debug  = true;

        private PlayerCharacterGUIController _controller;
        private CharacterInteractionAgent _pcAgent;
        
        private PlayerCharacterGUIController Controller => _controller ? _controller : _controller = GetComponent<PlayerCharacterGUIController>();

        private CharacterInteractionAgent PCAgent => Controller.HasAllResources()
            ? (_pcAgent != null ? _pcAgent : _pcAgent = Controller.PlayerCharacter.GetComponent<CharacterInteractionAgent>()) : null;
        
        private IEnumerator Start()
        {
            while (!Controller.HasAllResources())
            {
                if(debug)Debug.Log($"{nameof(UIPlayerInteractionController)} Waiting for resources",this);
                yield return null;
            }
            if(debug)Debug.Log($"{nameof(UIPlayerInteractionController)} All resources loaded",this);
        }
        

        private void Update()
        {
            if ((!Controller.HasAllResources())) 
                return;
            

            //read interact input from player input and pass to interaction agent
            PCAgent.InteractPressed = Controller.playerInput.actions["Interact"].WasPressedThisFrame();
            PCAgent.InteractHeld = Controller.playerInput.actions["Interact"].IsPressed();

            //TODO: once interaction system can get a list of available interactions for any interactable for UI

        }
    }
}