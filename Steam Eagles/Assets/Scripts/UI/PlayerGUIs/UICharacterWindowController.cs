using System;
using System.Collections.Generic;
using Characters;
using CoreLib.Entities;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.PlayerGUIs.CharacterWindows
{
    public class UICharacterWindowController : MonoBehaviour, IPCGUIController
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
        [Serializable]
        public class WindowEvents
        {
            public UnityEvent<UICharacterWindowBase> onEnter;
            public UnityEvent<UICharacterWindowBase> onLogic;
            public UnityEvent<UICharacterWindowBase> onExit;
        }
        
        [TableList,SerializeField] private CharacterWindowPanel[] windowPanels;

        public bool debug;
        private Dictionary<CharacterWindowState, UICharacterWindowBase> _windowPanels = new();
        private CharacterWindowState _characterWindowState = CharacterWindowState.CLOSED;
        public ReactiveProperty<CharacterWindowState> _onCharacterWindowStateChange = new ReactiveProperty<CharacterWindowState>();
        private PlayerInput _playerInput;
        private GameObject _characterGameObject;
        private StateMachine _stateMachineRoot;
        private StateMachine _characterStateMachine;

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

        CharacterInteractionState _characterInteractionState;

        public CharacterInteractionState characterInteractionState => _characterInteractionState ??=
            _characterGameObject.GetComponent<CharacterInteractionState>();

        bool HasRequirements() => _playerInput != null && 
                                  _characterGameObject != null && 
                                  characterInteractionState != null;

        public void SetCharacter(PlayerInput playerInput, GameObject characterGameObject)
        {
            _playerInput = playerInput;
            _characterGameObject = characterGameObject;
            if (WindowState != CharacterWindowState.CLOSED && !HasRequirements())
                WindowState = CharacterWindowState.CLOSED;
        }

        private void Awake()
        {
            foreach (var windowPanel in windowPanels)
            {
                _windowPanels.Add(windowPanel.window.GetWindowState(), windowPanel.window);
                windowPanel.guiButton.onClick.AsObservable().Subscribe(_ => ChangeToWindowState(windowPanel.window.GetWindowState())).AddTo(this);
            }

            _characterStateMachine = CreateWindowStateMachine();
            var fsm = new StateMachine();
            fsm.AddState("Character Window", OnCharacterWindowEnter, OnCharacterWindowLogic, OnCharacterWindowExit);
            fsm.AddState("Closed");
            fsm.AddTransitionFromAny("Character Window", _ => WindowState != CharacterWindowState.CLOSED);
            fsm.AddTransitionFromAny("Closed", _ => WindowState == CharacterWindowState.CLOSED);
            WindowState = CharacterWindowState.CLOSED;
            _stateMachineRoot = fsm;
            _stateMachineRoot.Init();
        }

       
        [FoldoutGroup("Events")]  public UnityEvent onCharacterWindowEnter;
       [FoldoutGroup("Events")]  public UnityEvent onCharacterWindowLogic;
       [FoldoutGroup("Events")]  public UnityEvent onCharacterWindowExit;

        public void OnCharacterWindowEnter(State<string, string> t)
        {
            _characterStateMachine.OnEnter();
            onCharacterWindowEnter.Invoke();
        }
        public void OnCharacterWindowLogic(State<string, string> t)
        {
            _characterStateMachine.OnLogic();
            onCharacterWindowLogic.Invoke();
        }
        public void OnCharacterWindowExit(State<string, string> t)
        {
            _characterStateMachine.OnExit();
            onCharacterWindowExit.Invoke();
        }

        private void Update()
        {
            if(!HasRequirements())
                return;
            _stateMachineRoot.OnLogic();
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

        [SerializeField, FoldoutGroup("Events")] private WindowEvents inventoryEvents;
        private void OnEnterInventory(State<string, string> t)
        {
            inventoryEvents.onEnter.Invoke(_windowPanels[CharacterWindowState.INVENTORY]);
            DebugStateEnter(CharacterWindowState.INVENTORY);
        }
        private void OnLogicInventory(State<string, string> t)
        {
            inventoryEvents.onLogic.Invoke(_windowPanels[CharacterWindowState.INVENTORY]);
            DebugStateLogic(CharacterWindowState.INVENTORY);
        }
        private void OnExitInventory(State<string, string> t)
        {
            inventoryEvents.onExit.Invoke(_windowPanels[CharacterWindowState.INVENTORY]);
            DebugStateExit(CharacterWindowState.INVENTORY);
        }

        #endregion

        
        #region [Map]

        [SerializeField, FoldoutGroup("Events")] private WindowEvents mapEvents;
        private void OnEnterMap(State<string, string> t)
        {
            DebugStateEnter(CharacterWindowState.MAP);
            mapEvents.onEnter.Invoke(_windowPanels[CharacterWindowState.MAP]);
        }
        private void OnLogicMap(State<string, string> t)
        {
            DebugStateLogic(CharacterWindowState.MAP);
            mapEvents.onLogic.Invoke(_windowPanels[CharacterWindowState.MAP]);
        }
        private void OnExitMap(State<string, string> t)
        {
            DebugStateExit(CharacterWindowState.MAP);
            mapEvents.onExit.Invoke(_windowPanels[CharacterWindowState.MAP]);
        }

        #endregion

        
        #region [Character]

        [SerializeField, FoldoutGroup("Events")] private WindowEvents characterEvents;
        private void OnEnterCharacters(State<string, string> t)
        {
            characterEvents.onEnter.Invoke(_windowPanels[CharacterWindowState.CHARACTERS]);
            DebugStateEnter(CharacterWindowState.CHARACTERS);
        }
        private void OnLogicCharacters(State<string, string> t)
        {
            characterEvents.onLogic.Invoke(_windowPanels[CharacterWindowState.CHARACTERS]);
            DebugStateLogic(CharacterWindowState.CHARACTERS);
        }
        private void OnExitCharacters(State<string, string> t)
        {
            characterEvents.onExit.Invoke(_windowPanels[CharacterWindowState.CHARACTERS]);
            DebugStateExit(CharacterWindowState.CHARACTERS);
        }

        #endregion


        #region [Codex]
        [SerializeField, FoldoutGroup("Events")] private WindowEvents codexEvents;
        private void OnEnterCodex(State<string, string> t)
        {
            codexEvents.onEnter.Invoke(_windowPanels[CharacterWindowState.CODEX]);
            DebugStateEnter(CharacterWindowState.CODEX);
        }
        private void OnLogicCodex(State<string, string> t)
        {
            codexEvents.onLogic.Invoke(_windowPanels[CharacterWindowState.CODEX]);
            DebugStateLogic(CharacterWindowState.CODEX);
        }
        private void OnExitCodex(State<string, string> t)
        {
            codexEvents.onExit.Invoke(_windowPanels[CharacterWindowState.CODEX]);
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