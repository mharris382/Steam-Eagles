using System;
using CoreLib.Entities;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterWindowController : MonoBehaviour
    {
        private const string Inventory = "Inventory";
        private const string Codex = "Codex";
        private const string Map = "Map";
        private const string Characters = "Characters";


        [Serializable]
        public class CharacterWindowPanel
        {
            [Required,ChildGameObjectsOnly] public UICharacterWindowBase window;
            [Required,ChildGameObjectsOnly] public Button guiButton;
        }

        [TableList]
        [SerializeField] private CharacterWindowPanel[] windowPanels;

        public bool debug;
        private CharacterWindowState _characterWindowState = CharacterWindowState.CLOSED;
        public ReactiveProperty<CharacterWindowState> _onCharacterWindowStateChange = new ReactiveProperty<CharacterWindowState>();

        private PlayerInput _playerInput;

        private Entity _entity;

        private GameObject _characterGameObject;



        public CharacterWindowState WindowState
        {
            get => _characterWindowState;
            set
            {
                _characterWindowState = value;
                _onCharacterWindowStateChange.Value = _characterWindowState;
            }
        }

        public IReadOnlyReactiveProperty<CharacterWindowState> OnCharacterWindowStateChange =>
            _onCharacterWindowStateChange;

        public StateMachine CreateWindowStateMachine()
        {
            var stateMachine = new StateMachine();
            stateMachine.AddState(Inventory, OnEnterInventory, OnLogicInventory, OnExitInventory);
            stateMachine.AddState(Codex, OnEnterCodex, OnLogicCodex, OnExitCodex);
            stateMachine.AddState(Map, OnEnterMap, OnLogicMap, OnExitMap);
            stateMachine.AddState(Characters, OnEnterCharacters, OnLogicCharacters, OnExitCharacters);
            
            stateMachine.AddTransitionFromAny(Inventory, _ => _characterWindowState == CharacterWindowState.INVENTORY);
            stateMachine.AddTransitionFromAny(Codex, _ => _characterWindowState == CharacterWindowState.CODEX);
            stateMachine.AddTransitionFromAny(Map, _ => _characterWindowState == CharacterWindowState.MAP);
            stateMachine.AddTransitionFromAny(Characters, _ => _characterWindowState == CharacterWindowState.CHARACTERS);
            stateMachine.SetStartState(Inventory);
            stateMachine.Init();
            return stateMachine;
        }


        bool HasRequirements() => _playerInput != null && 
                                  _entity != null &&
                                  _characterGameObject != null;

        public void SetCharacter(PlayerInput playerInput, Entity entity, GameObject characterGameObject)
        {
            _playerInput = playerInput;
            _entity = entity;
            _characterGameObject = characterGameObject;
            if (WindowState != CharacterWindowState.CLOSED && !HasRequirements())
                WindowState = CharacterWindowState.CLOSED;
        }

        private void Update()
        {
            if(!HasRequirements())
                return;
            foreach (var windowPanel in windowPanels)
            {
                var state = windowPanel.window.GetWindowState();
                var action = windowPanel.window.GetActionName();
                if (_playerInput.actions[action].WasPressedThisFrame())
                {
                    if (windowPanel.window.IsVisible && state == windowPanel.window.GetWindowState())
                    {
                        windowPanel.window.Close();
                        WindowState = CharacterWindowState.CLOSED;
                    }
                    else
                    {
                        ChangeToWindowState(state);
                    }
                    return;
                }
            }
        }

        private void ChangeToWindowState(CharacterWindowState state)
        {
            foreach (var characterWindowPanel in windowPanels)
            {
                if (characterWindowPanel.window.GetWindowState() == state)
                {
                    characterWindowPanel.window.Open();
                }
                else
                {
                    characterWindowPanel.window.Close();
                }
            }
            WindowState = state;
        }

        #region [Inventory]

        private void OnEnterInventory(State<string, string> t)
        {
            DebugStateEnter(CharacterWindowState.INVENTORY);
        }
        private void OnLogicInventory(State<string, string> t)
        {
            DebugStateLogic(CharacterWindowState.INVENTORY);
        }
        private void OnExitInventory(State<string, string> t)
        {
            DebugStateExit(CharacterWindowState.INVENTORY);
        }

        #endregion

        
        #region [Map]

        private void OnEnterMap(State<string, string> t)
        {
            DebugStateEnter(CharacterWindowState.MAP);
        }
        private void OnLogicMap(State<string, string> t)
        {
            DebugStateLogic(CharacterWindowState.MAP);
        }
        private void OnExitMap(State<string, string> t)
        {
            DebugStateExit(CharacterWindowState.MAP);
        }

        #endregion

        
        #region [Character]

        private void OnEnterCharacters(State<string, string> t)
        {
            DebugStateEnter(CharacterWindowState.CHARACTERS);
        }
        private void OnLogicCharacters(State<string, string> t)
        {
            DebugStateLogic(CharacterWindowState.CHARACTERS);
        }
        private void OnExitCharacters(State<string, string> t)
        {
            DebugStateExit(CharacterWindowState.CHARACTERS);
        }

        #endregion


        #region [Codex]

        private void OnEnterCodex(State<string, string> t)
        {
            DebugStateEnter(CharacterWindowState.CODEX);
        }
        private void OnLogicCodex(State<string, string> t)
        {
            DebugStateLogic(CharacterWindowState.CODEX);
        }
        private void OnExitCodex(State<string, string> t)
        {
            DebugStateExit(CharacterWindowState.CODEX);
        }

        #endregion
        
        
        public void DebugStateEnter(CharacterWindowState state)
        {
            if(debug)
                Debug.Log($"Enter: {state}");
        }
        
        public void DebugStateExit(CharacterWindowState state)
        {
            if(debug)
                Debug.Log($"Exit: {state}");
        }
        public void DebugStateLogic(CharacterWindowState state)
        {
            if(debug)
                Debug.Log($"OnLogic: {state}");
        }

        public enum CharacterWindowState
        {
            CLOSED,
            INVENTORY,
            CODEX,
            MAP,
            CHARACTERS
        }
    }
}