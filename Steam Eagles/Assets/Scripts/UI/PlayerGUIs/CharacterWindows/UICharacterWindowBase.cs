using System;
using CoreLib.Entities;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs.CharacterWindows
{
    public abstract class UICharacterWindowBase : Window
    {
        private PlayerCharacterGUIController _characterGUI;
        public PlayerInput PlayerInput => _characterGUI.playerInput;
        public GameObject Character => _characterGUI.PlayerCharacter;
        public Entity PlayerCharacterEntity => Character.GetComponent<Entity>();
        
        public abstract string GetActionName();
        
        public abstract UICharacterWindowController.CharacterWindowState GetWindowState();

        public override void Init()
        {
            _characterGUI = GetComponentInParent<PlayerCharacterGUIController>();
            base.Init();
        }


        public bool HasResources() => _characterGUI != null && 
                                      _characterGUI.PlayerCharacter != null && 
                                      _characterGUI.playerInput != null;

        public override void Open()
        {
            if (HasResources())
            {
                InitializeWindow(Character, PlayerInput);
            }
            base.Open();
            MessageBroker.Default.Publish(new UICharacterWindowOpened(this));
        }

        public override void Close()
        {
            if (HasResources())
            {
                CleanUpWindow(Character, PlayerInput);
            }
            base.Close();
            MessageBroker.Default.Publish(new UICharacterWindowClosed(this));
        }

        protected virtual void InitializeWindow(GameObject character, PlayerInput playerInput)
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        public virtual void CleanUpWindow(GameObject character, PlayerInput playerInput)
        {
            playerInput.SwitchCurrentActionMap("Gameplay");
        }

        private void Update()
        {
            if (!HasResources())
            {
                Debug.Log($"Window: {name} is waiting for resources");
                return;
            }

        }
    }

    
    public struct UICharacterWindowClosed
    {
        public UICharacterWindowBase CharacterWindow { get; }
        public PlayerInput PlayerInput { get; }

        public UICharacterWindowClosed(UICharacterWindowBase characterWindow)
        {
            CharacterWindow = characterWindow;
            PlayerInput = characterWindow.PlayerInput;
        }
    }
    public struct UICharacterWindowOpened
    {
        public UICharacterWindowBase CharacterWindow { get; }
        public PlayerInput PlayerInput { get; }

        public UICharacterWindowOpened(UICharacterWindowBase characterWindow)
        {
            CharacterWindow = characterWindow;
            PlayerInput = characterWindow.PlayerInput;
        }
    }
}