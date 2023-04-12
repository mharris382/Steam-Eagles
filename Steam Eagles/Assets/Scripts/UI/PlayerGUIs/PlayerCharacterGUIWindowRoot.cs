using System;
using CoreLib.Entities;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using FSM;
using UI.PlayerGUIs.CharacterWindows;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(UIWindowPanel))]
    public partial class PlayerCharacterGUIWindowRoot : Window
    {
        
        private UIWindowPanel _windowPanel;
        
        public UIWindowPanel WindowPanel => _windowPanel ? _windowPanel : _windowPanel = GetComponent<UIWindowPanel>();
        protected override bool UsesCloseButton => false;
        protected override bool BlockRaycastsWhenVisible => false;

        [Required, SerializeField] private PlayerCharacterHUD playerCharacterHUD;
        [Required, SerializeField] private UICharacterWindowController characterWindowController;

        private ReactiveProperty<GUIState> _guiState;
        private ReactiveProperty<GUIState> guiState => _guiState ??= new ReactiveProperty<GUIState>(GUIState.DISABLED);


        public enum GUIState
        {
            DISABLED,
            NORMAL_GAMEPLAY_HUD,
            CHARACTER_WINDOW
        }

        StateMachine _stateMachine;
        private PlayerInput _input;
        private Entity _entity;
        private GameObject _characterGameObject;

        public bool HasRequirements() => _input != null && 
                                         _entity != null &&
                                         _characterGameObject != null;
        
        private void Awake()
        {
            _guiState = new ReactiveProperty<GUIState>(GUIState.DISABLED);
            _stateMachine = new StateMachine();
            _stateMachine.AddState("DISABLED", new PlayerGUIDisabledState(this, playerCharacterHUD));
            _stateMachine.AddState("HUD", new PlayerHUDState(this, playerCharacterHUD));
            _stateMachine.AddState("CHARACTER_WINDOW", new PlayerCharacterWindowState(this, playerCharacterHUD, characterWindowController));
            
            _stateMachine.AddTransitionFromAny("HUD", _ => guiState.Value == GUIState.NORMAL_GAMEPLAY_HUD);
            _stateMachine.AddTransition("CHARACTER_WINDOW", "HUD", _ => characterWindowController.WindowState == UICharacterWindowController.CharacterWindowState.CLOSED);
            _stateMachine.AddTransition("HUD", "CHARACTER_WINDOW", _ => characterWindowController.WindowState != UICharacterWindowController.CharacterWindowState.CLOSED);
            _stateMachine.AddTransitionFromAny("DISABLED", _ => guiState.Value == GUIState.DISABLED);
            
            _stateMachine.SetStartState("DISABLED");
            _stateMachine.Init();

            characterWindowController.OnCharacterWindowStateChange
                .Select(t => t != UICharacterWindowController.CharacterWindowState.CLOSED)
                .Subscribe(isCharacterWindowOpen => _guiState.Value = isCharacterWindowOpen ? GUIState.CHARACTER_WINDOW : GUIState.NORMAL_GAMEPLAY_HUD)
                .AddTo(this);

            _guiState.Select(t => t != GUIState.DISABLED)
                .Subscribe(enable => characterWindowController.enabled = enable)
                .AddTo(this);
            characterWindowController.enabled = _guiState.Value != GUIState.DISABLED;
        }

        private void Update()
        {
            CheckGUIState();
            _stateMachine.OnLogic();
        }

        private void CheckGUIState()
        {
            if (_guiState.Value != GUIState.DISABLED && !HasRequirements())
            {
                _guiState.Value = GUIState.DISABLED;
            }
            else if (_guiState.Value == GUIState.DISABLED && HasRequirements())
            {
                _guiState.Value = GUIState.NORMAL_GAMEPLAY_HUD;
            }
        }

        public void DisableWindow()
        {
            Close();
            playerCharacterHUD.Close();
        }

        public void SetWindowActive(PlayerInput playerInput, Entity characterEntity, GameObject characterEntityGameObject)
        {
            Debug.Log($"SetWindowActive: \nPlayerInput:{playerInput},\nEntity:{characterEntity}, \nCharacter:{characterEntityGameObject}");
            this._input = playerInput;
            this._entity = characterEntity;
            this._characterGameObject = characterEntityGameObject;
            
            playerCharacterHUD.LinkHUDToEntity(characterEntity, characterEntityGameObject);
            characterWindowController.SetCharacter(playerInput, characterEntity, characterEntityGameObject);
            
            Open();
            _guiState.Value = GUIState.NORMAL_GAMEPLAY_HUD;
            //TODO: determine if we should show the HUD or the character window
        }
    }
}