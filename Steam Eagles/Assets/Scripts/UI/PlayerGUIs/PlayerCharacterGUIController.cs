using System;
using System.Collections;
using CoreLib;
using CoreLib.Entities;
using CoreLib.Signals;
using Cysharp.Threading.Tasks;
using Game;
using Players.Shared;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;
using PlayerInput = UnityEngine.InputSystem.PlayerInput;
// ReSharper disable InconsistentNaming

namespace UI.PlayerGUIs
{
    public enum PCGUIState
    {
        WAITING_FOR_CHARACTER,
        LOADING,
        SAVING,
        PAUSE_MENU,
        CHARACTER_MENU,
        CUTSCENE,
        PILOTING,
        DEFAULT
    }
    public enum UIInputMode
    {
        NONE,
        KEYBOARD_MOUSE,
        CONTROLLER
    }
    public class PlayerCharacterGUIController : MonoBehaviour
    {
        [OnValueChanged(nameof(OnPlayerIDChanged)), Range(0, 1), SerializeField] private int playerID;
        
        [Obsolete]
        [SerializeField, ChildGameObjectsOnly, Required, Tooltip("Root window of the player GUI. Holds all other windows")]
        private PlayerCharacterGUIWindowRoot rootWindow;

        [Obsolete] private IDisposable _entityListener;
        [Obsolete] private ReactiveProperty<bool> _isGuiActive = new ReactiveProperty<bool>(false);
        [Obsolete] private ReactiveProperty<Entity> __pcEntity = new ReactiveProperty<Entity>();
        
        [Obsolete]
        private Coroutine _initialization;
        private PCRegistry _pcRegistry;

        
        private ReadOnlyReactiveProperty<GameObject> _characterGameObject;
        private ReadOnlyReactiveProperty<PlayerInput> _pInput;
        private ReadOnlyReactiveProperty<Camera> _playerCamera;
        private ReadOnlyReactiveProperty<UIInputMode> _uiInputMode;
        private PCGUIState _guiState;


        #region [Debugging Properties]

        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")] public PlayerInput playerInput => _pInput?.Value;
        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")] public Camera PlayerCamera => _playerCamera?.Value;
        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")]public GameObject PlayerCharacter => _characterGameObject?.Value;
        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")] public UIInputMode InputMode => _uiInputMode?.Value ?? UIInputMode.NONE;
        
        #endregion

        public IReadOnlyReactiveProperty<PlayerInput> PlayerInputProperty => _pInput;
        
        public IReadOnlyReactiveProperty<Camera> PlayerCameraProperty => _playerCamera;
        
        public IReadOnlyReactiveProperty<GameObject> PlayerCharacterProperty => _characterGameObject;

        public IReadOnlyReactiveProperty<UIInputMode> InputModeProperty => _uiInputMode;

        public PCGUIState GUIState
        {
            get => IsWaitingForCharacter() ? PCGUIState.WAITING_FOR_CHARACTER : _guiState;
            set => _guiState = value;
        }
        
        
        #region [Obsolete Properties]

        [Obsolete] public IReadOnlyReactiveProperty<Entity> PcEntityProperty => __pcEntity ??= new ReactiveProperty<Entity>();

        [Obsolete("Should not be managed at root level")][SerializeField] private BoolReactiveProperty showRecipeHUD = new BoolReactiveProperty(true);
        [Obsolete("Should not be managed at root level")] public IReadOnlyReactiveProperty<bool> ShowRecipeHUD => showRecipeHUD;
        [Obsolete] public Entity pcEntity => null;

        #endregion


        [Inject]
        void Inject(PCRegistry pcRegistry)
        {
            _pcRegistry = pcRegistry;

            var onAdded = _pcRegistry.OnValueAdded.Where(t => t.PlayerNumber == this.playerID);
            onAdded.Subscribe(t => t.PC.hud = gameObject).AddTo(this);
            var onInputAdded =onAdded.Select(t => t.PC.input.GetComponent<PlayerInput>()).Where(t => t != null);
            var onCameraAdded = onAdded.Select(t => t.PC.camera.GetComponent<Camera>()).Where(t => t != null);
            var onCharacterAdded = onAdded.Select(t => t.PC.character);

            var onRemoved = _pcRegistry.OnValueRemoved.Where(t => t.PlayerNumber == this.playerID);
            var onInputRemoved = onRemoved.Select(_ => _pInput.Value);
            var onCameraRemoved = onRemoved.Select(_ => _playerCamera.Value);
            var onCharacterRemoved = onRemoved.Select(_ => (GameObject)null);

            _uiInputMode =  onInputAdded
                .Do(input => Debug.Assert(input != null, "Input null", this))
                .Where(t => t != null)
                .Select(input => input.currentControlScheme.Contains("Keyboard") ? UIInputMode.KEYBOARD_MOUSE : UIInputMode.CONTROLLER)
                .Merge( onInputRemoved.Select(_ => UIInputMode.NONE)).ToReadOnlyReactiveProperty();
           
            
            _characterGameObject =onCharacterAdded.Merge(onCharacterRemoved).ToReadOnlyReactiveProperty();
            _pInput = onInputAdded.Merge(onInputRemoved).ToReadOnlyReactiveProperty();
            
            _playerCamera = onCameraAdded.Merge(onCameraRemoved).ToReadOnlyReactiveProperty();
        }


   
        private IEnumerator Start()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Log("Waiting for DI...");
                await UniTask.WaitUntil(() => _pcRegistry != null);
                Log("Waiting for all UI resources...");
                await UniTask.WaitUntil(HasAllResources);
                Log("Waiting for all character to finish loading...");
                var entityInitializer = this.PlayerCharacter.GetComponent<EntityInitializer>();
                await UniTask.WaitUntil(() => entityInitializer.isDoneInitializing);
                var guiControllers = GetComponentsInChildren<IPCGUIController>(true);
                Debug.Assert(guiControllers.Length > 0, $"No GUI Controllers found on {name}", this);
                foreach (var pcGuiController in guiControllers)
                    pcGuiController.SetCharacter(this.playerInput, this.PlayerCharacter);
            });
        }

        

        public bool HasAllResources() => _pcRegistry != null && _pcRegistry.HasPc(this.playerID);

        public bool IsWaitingForCharacter()
        {
            if (_pcRegistry == null)
                return true;
            return !_pcRegistry.HasPc(playerID);
        }


        void Log(string msg) => Debug.Log($"P{playerInput} UI: {msg}", this);

        #region [Editor]

        void OnPlayerIDChanged(int value) => name = $"PLAYER {value} GUI";


        [BoxGroup("Debug")]
        [ShowInInspector]
        public bool IsActive
        {
            get
            {
                if (!Application.isPlaying)
                    return false;
                if(_isGuiActive == null)_isGuiActive = new ReactiveProperty<bool>(false);
                return _isGuiActive.Value;
            }
        }
        
        #endregion
        
        
        
        public void CharacterWindowOpened()
        {
            GUIState = PCGUIState.CHARACTER_MENU;
        }

        
        public void CharacterWindowClosed()
        {
            GUIState = PCGUIState.DEFAULT;
        }
    }


    public interface IPCGUIController
    {
        void SetCharacter(PlayerInput input, GameObject characterGo);
    }
}