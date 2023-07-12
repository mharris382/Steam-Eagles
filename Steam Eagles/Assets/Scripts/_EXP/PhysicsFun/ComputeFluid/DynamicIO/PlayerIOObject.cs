using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _EXP.PhysicsFun.ComputeFluid
{
    public class PlayerIOObject : DynamicIOObject
    {
        [Range(0, 1)] public int player;
        private PlayerInput _playerInput;
        private EntityRoomState _entityRoomState;
        private Room _room;

        [ShowInInspector, BoxGroup("Debug")]
        public bool IsPlayerInRoom
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debug")]
        public float PlayerGasValue
        {
            get;
            set;
        }
        
        void Awake()
        {
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().Select(t =>
                    (t.characterState.GetComponent<EntityRoomState>(), t.inputGo.GetComponent<PlayerInput>()))
                .Subscribe(t =>
                {
                    _entityRoomState = t.Item1;
                    _playerInput = t.Item2;
                }).AddTo(this);
        }

        [Inject] void Install(Room room)
        {
            _room = room;
        }

        private void Update()
        {
            if (_playerInput == null || _entityRoomState == null || _room == null)
                return;

            var room = _entityRoomState.CurrentRoom.Value;
            IsPlayerInRoom = room == _room;
            if (!IsPlayerInRoom)
            {
                PlayerGasValue = 0;
                return;
            }
            transform.position = _entityRoomState.transform.position;
            var value = _playerInput.actions["Gas"].ReadValue<float>();
            PlayerGasValue = value;
        }

        public override float GetTargetGasIOValue()
        {
            if (_playerInput == null || _entityRoomState == null)
                return 0;
            return IsPlayerInRoom ?  PlayerGasValue : 0;
        }
    }
}