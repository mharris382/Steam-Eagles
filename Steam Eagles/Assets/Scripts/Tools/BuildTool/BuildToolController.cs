using System;
using System.Collections;
using Buildings;
using Buildings.Messages;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Items;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class BuildToolController : MonoBehaviour
    {
        public Building targetBuilding;

        
        private Character _character;
        private Inventory _inventory;
        private EntityRoomState _roomState;

        public TilePathTool pathTool;
        
        
        private BoolReactiveProperty _hasRoom = new BoolReactiveProperty(false);
        
        private void Awake()
        {
            _character = GetComponentInParent<Character>();
            
            if(!_character.gameObject.TryGetComponent(out _roomState))
                _roomState = _character.gameObject.AddComponent<EntityRoomState>();
            
            var allInventories = _character.GetComponentsInChildren<Inventory>();
            foreach (var inventory in allInventories)
            {
                if (inventory.isMain)
                {
                    _inventory = inventory;
                    break;
                }
            }

            if (_inventory == null)
                Debug.LogError($"Couldn't find main inventory on character{_character.name}", _character);
            _hasRoom = new BoolReactiveProperty(false);
            
        }

        private IEnumerator Start()
        {
            while (!_character.IsEntityInitialized)
                yield return null;
            _roomState.CurrentRoom.Subscribe(OnRoomChanged).AddTo(this);
            OnRoomChanged(_roomState.CurrentRoom.Value);
        }


        void OnRoomChanged(Room room)
        {
            if (room == null)
            {
                pathTool.enabled = false;
                return;
            }
            pathTool.enabled = room.buildLevel == BuildLevel.FULL;
        }
    }
}