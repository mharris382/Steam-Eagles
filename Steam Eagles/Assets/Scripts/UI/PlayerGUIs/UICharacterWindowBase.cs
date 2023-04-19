using System;
using CoreLib.Entities;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs.CharacterWindows
{
    public abstract class UICharacterWindowBase : Window
    {
        
        [Serializable]
        public class Selectable
        {
            [SerializeField] private SelectMode selectMode;
            
            [ShowIf("@selectMode == SelectMode.FIXED"), Required]
            [SerializeField] private GameObject fixedObject;
            
            [ShowIf("@selectMode == SelectMode.FIRST_CHILD"), Required]
            [SerializeField] private RectTransform parent;

            public GameObject GetDefaultSelectable => selectMode == SelectMode.FIRST_CHILD ? parent.GetChild(0).gameObject : fixedObject;
            private enum SelectMode
            {
                FIXED,
                FIRST_CHILD
            }
        }
        
        
        [SerializeField] private Selectable selectable;

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
                var playerInput = PlayerInput;
                Debug.Assert(playerInput.uiInputModule != null, "PlayerInput has no UIInputModule", playerInput.uiInputModule);
                var eventSystem = playerInput.uiInputModule.GetComponent<EventSystem>();
                Debug.Assert(eventSystem != null, "PlayerInput has no EventSystem", playerInput);
                eventSystem.SetSelectedGameObject(selectable.GetDefaultSelectable);
                MessageBroker.Default.Publish(new UICharacterWindowOpened(this));
            }
            base.Open();
        }

        public override void Close()
        {
            if (HasResources())
            {
                CleanUpWindow(Character, PlayerInput);
                MessageBroker.Default.Publish(new UICharacterWindowClosed(this));
            }

            base.Close();
        }

        protected virtual void InitializeWindow(GameObject character, PlayerInput playerInput)
        {
        }

        public virtual void CleanUpWindow(GameObject character, PlayerInput playerInput)
        {
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