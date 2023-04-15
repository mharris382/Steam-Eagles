using System;
using System.Collections;
using Buildings;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Items;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public abstract class ToolControllerBase : MonoBehaviour
    {
        public Building targetBuilding;
        private CharacterState _characterState;
        private Inventory _inventory;
        private EntityRoomState _roomState;
        private BoolReactiveProperty _hasRoom = new BoolReactiveProperty(false);

        protected CharacterState CharacterState => _characterState;
        protected Inventory Inventory => _inventory;
        protected EntityRoomState RoomState => _roomState;
        public bool HasRoom
        {
            get => _hasRoom.Value;
            protected set => _hasRoom.Value = value;
        }
        private void Awake()
        {
            _characterState = GetComponentInParent<CharacterState>();
            
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
            _roomState.CurrentRoom.Subscribe(OnRoomChanged).AddTo(this);
            OnRoomChanged(_roomState.CurrentRoom.Value);
            OnStart();
        }

        protected virtual void OnStart()
        {
        }



        protected abstract void OnRoomChanged(Room room);

        public void Activate(Tool tool)
        {
            throw new NotImplementedException();
        }
    }



   
}