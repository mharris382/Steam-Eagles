﻿using System;
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
    public class PlayerCharacterGUIController : MonoBehaviour
    {
        [OnValueChanged(nameof(OnPlayerIDChanged)), Range(0, 1), SerializeField] private int playerID;
        [SerializeField, ChildGameObjectsOnly, Required, Tooltip("Root window of the player GUI. Holds all other windows")] private PlayerCharacterGUIWindowRoot rootWindow;

        [Obsolete] private IDisposable _entityListener;
        [Obsolete] private ReactiveProperty<bool> _isGuiActive = new ReactiveProperty<bool>(false);
        [Obsolete] private ReactiveProperty<Entity> __pcEntity = new ReactiveProperty<Entity>();
        [Obsolete] private ReactiveProperty<PlayerInput> __playerInput = new ReactiveProperty<PlayerInput>();
        
        private ReadOnlyReactiveProperty<GameObject> _characterGameObject;
        private ReadOnlyReactiveProperty<PlayerInput> _pInput;
        private ReadOnlyReactiveProperty<Camera> _playerCamera;



        [Obsolete("Should not be managed at root level")][SerializeField] private BoolReactiveProperty showRecipeHUD = new BoolReactiveProperty(true);
        [Obsolete("Should not be managed at root level")] public IReadOnlyReactiveProperty<bool> ShowRecipeHUD => showRecipeHUD;

  

        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")] public PlayerInput playerInput => _pInput?.Value;
        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")] public Camera PlayerCamera => _playerCamera?.Value;
        [ShowInInspector, ReadOnly,HideInEditorMode,BoxGroup("Debugging")]public GameObject PlayerCharacter => _characterGameObject?.Value;
  
        [Obsolete] public Entity pcEntity => null;

        public IReadOnlyReactiveProperty<PlayerInput> PlayerInputProperty => _pInput;
        
        public IReadOnlyReactiveProperty<Camera> PlayerCameraProperty => _playerCamera;
        
        public IReadOnlyReactiveProperty<GameObject> PlayerCharacterProperty => _characterGameObject;

        [Obsolete] public IReadOnlyReactiveProperty<Entity> PcEntityProperty => __pcEntity ??= new ReactiveProperty<Entity>();

        


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

            _characterGameObject =onCharacterAdded.Merge(onCharacterRemoved).ToReadOnlyReactiveProperty();
            _pInput = onInputAdded.Merge(onInputRemoved).ToReadOnlyReactiveProperty();
            _playerCamera = onCameraAdded.Merge(onCameraRemoved).ToReadOnlyReactiveProperty();
        }


        [Obsolete]
        private Coroutine _initialization;
        private PCRegistry _pcRegistry;

        private IEnumerator Start()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Log("Waiting for DI...");
                await UniTask.WaitUntil(() => _pcRegistry != null);
                Log("Waiting for all UI resources...");
                await UniTask.WaitUntil(HasAllResources);
            });
        }

        

        public bool HasAllResources() => _pcRegistry != null && _pcRegistry.HasPc(this.playerID);

        private void OnDestroy()
        {
       
        }

        private void UpdateGUIActiveState() => _isGuiActive.Value = HasAllResources();


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
    }
}