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
        private ToolControllerSharedData _toolData;
        private bool _isActive;

        protected CharacterState CharacterState => _characterState;
        protected Inventory Inventory => _inventory;
        protected EntityRoomState RoomState => _roomState;


        
        
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

        public void SetActive(bool isActive)
        {
            if (isActive != _isActive)
            {
                _isActive = isActive;
                activationEvents.OnActivationStateChanged(isActive);
            }
        }
        
        private void Awake()
        {
            _isActive = false;
            _characterState = GetComponentInParent<CharacterState>();
            var inputState = _characterState.GetComponentInChildren<CharacterInputState>();
            _toolData = GetComponentInParent<ToolControllerSharedData>();

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