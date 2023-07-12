using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.BuildingTilemaps;
using Buildings.Rooms;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public abstract class Machine<T> : MonoBehaviour where T : Machine<T>
    {
        private MachineHandler<T> _handlers;
        private BuildableMachineBase _buildable;
        private Building _building;
        
        public Building Building => _building ??= GetComponentInParent<Building>();
        public BuildableMachineBase Buildable => _buildable ??= GetComponent<BuildableMachineBase>();

        [Inject] void Install(MachineHandler<T> handlers)
        {
            if (enabled) handlers.Register(this as T);
            _handlers = handlers;
        }
        private void OnEnable()
        {
            if (_handlers != null) {_handlers.Register(this as T);}
            SetRoomAsParent();
            DoOnEnable();
        }
        private void OnDisable()
        {
            if (_handlers != null) _handlers.Unregister(this as T);
            DoOnDisable();
        }
        
        private void SetRoomAsParent()
        {
            var overlappingRooms = FindOverlappingRooms().First();
            var room = overlappingRooms.Item1;
            transform.parent = room.transform;
        }
        
        public IEnumerable<(Room, BuildingCell)> FindOverlappingRooms()
        {
            var cells = Buildable.GetCells();
            var map = Building.Map;
            foreach (var machineCell in cells)
            {
                var cell = new BuildingCell(machineCell, BuildingLayers.SOLID);
                var room = map.GetRoom(cell);
                if (room != null) yield return (room, cell);
            }
        }


        protected virtual void DoOnEnable(){ }
        protected virtual void DoOnDisable(){ }
        
        
        
    }


    public interface IMachinePipeIOPort
    {
        BuildingCell Cell { get; }
        
        bool IsConnected { set; }

        IOPortMode Mode() => ModeProperty.Value;
        IReadOnlyReactiveProperty<IOPortMode> ModeProperty { get; }
        
        float GetTargetGasIOValue();
        
        void OnGasIO(float gasDelta);
    }
}