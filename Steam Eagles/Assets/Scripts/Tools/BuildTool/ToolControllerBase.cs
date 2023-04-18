using System;
using System.Collections;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Characters;
using CoreLib;
using Items;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Tools.BuildTool
{
    public abstract class ToolControllerBase : MonoBehaviour
    {
        [SerializeField] private ActivationEvents activationEvents;
        
        public Building targetBuilding;
        private CharacterState _characterState;
        private Inventory _inventory;
        private EntityRoomState _roomState;
        private BoolReactiveProperty _hasRoom = new BoolReactiveProperty(false);

        protected CharacterState CharacterState => _characterState;
        protected Inventory Inventory => _inventory;
        protected EntityRoomState RoomState => _roomState;


        private BoolReactiveProperty _isInitialized;
        private BoolReactiveProperty _isActivated;
        private ToolControllerSharedData _toolData;

        
        [Serializable]
        public class ActivationEvents
        {
            public UnityEvent onActivated;
            public UnityEvent onDeactivated;
            public UnityEvent<bool> onActivationStateChanged;
            public void OnActivationStateChanged(bool active)
            {
                onActivationStateChanged.Invoke(active);
                if (active)
                    onActivated.Invoke();
                else
                    onDeactivated.Invoke();
            }
        }

        public bool HasRoom
        {
            get => _hasRoom.Value;
            protected set => _hasRoom.Value = value;
        }
        private void Awake()
        {
            _characterState = GetComponentInParent<CharacterState>();
            var inputState = _characterState.GetComponentInChildren<CharacterInputState>();
            _toolData = GetComponentInParent<ToolControllerSharedData>();


            _isActivated = new BoolReactiveProperty();
            _isInitialized = new BoolReactiveProperty();
            _isActivated.StartWith(false)
                .ZipLatest(_isInitialized.StartWith(false),
                    (active, inited) =>
                    {
                        Debug.Log($"{name} Received activation state change: Equipped:{active} Initialized:{inited}",this);
                        return active && inited;
                    })
                .DistinctUntilChanged()
                .Subscribe(activationEvents.OnActivationStateChanged)
                .AddTo(this);
            
            if (_toolData.ActiveTool == null)
            {
                _toolData.ActiveTool.Value = this;
            }
            
            Debug.Assert(_toolData != null, "ToolControllerSharedData not found", this);
            _toolData.RegisterTool(this);
            if(!_characterState.gameObject.TryGetComponent(out _roomState))
                _roomState = _characterState.gameObject.AddComponent<EntityRoomState>();
            
            var allInventories = _characterState.GetComponentsInChildren<Inventory>();
            foreach (var inventory in allInventories)
            {
                if (inventory.isMain)
                {
                    _inventory = inventory;
                    break;
                }
            }

            if (_inventory == null)
                Debug.LogError($"Couldn't find main inventory on character{_characterState.name}", _characterState);
            _hasRoom = new BoolReactiveProperty(false);
            OnAwake();
        }
        
        protected virtual void OnAwake(){}
        
        public bool HasResources()
        {
            return _inventory != null && _characterState != null && _roomState != null;
        }

        private IEnumerator Start()
        {
            while (!_characterState.IsEntityInitialized)
                yield return null;
            if (_toolData.ActiveTool.Value == null)
            {
                _toolData.ActiveTool.Value = this;
            }
            _toolData.ActiveTool.Select(t=> t == this)
                .StartWith(_toolData.ActiveTool.Value == this)
                .Subscribe(isActive=> this._isActivated.Value = isActive)
                .AddTo(this);
            
            _roomState.CurrentRoom.Subscribe(OnRoomChanged).AddTo(this);
            OnRoomChanged(_roomState.CurrentRoom.Value);
            OnStart();
        }

        protected virtual void OnStart()
        {
        }


        private void OnDestroy()
        {
            _toolData.UnregisterTool(this);
        }

        protected abstract void OnRoomChanged(Room room);

        public void Activate(Tool tool)
        {
            this._toolData.AddTool(tool);
            //throw new NotImplementedException();
        }

        public abstract ToolStates GetToolState();
        
        public abstract bool UsesRecipes(out List<Recipe> recipes);
    }



   
}