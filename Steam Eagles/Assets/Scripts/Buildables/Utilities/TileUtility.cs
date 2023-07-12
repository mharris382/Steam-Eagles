using Buildables.Interfaces;
using Buildings;
using Buildings.Rooms;
using UnityEngine;

namespace Buildables.Utilities
{
    public abstract class TileUtility : MonoBehaviour, IMachineListener
    {
        
        public abstract BuildingLayers TargetLayer { get; }
        private BuildableMachineBase _machine;
        private Room _room;
        private Building _building;
        
        public BuildingCell Cell => Machine.Building == null ? default : new BuildingCell(GridPosition, TargetLayer);
        public Vector3Int GridPosition=> Machine.Building == null ? default : Machine.Building.Map.WorldToCell(transform.position, TargetLayer);
        public BuildableMachineBase Machine => _machine ??= GetComponentInParent<BuildableMachineBase>();
        public Room Room { get; private set; }
        public Building Building => Machine.Building;

        public abstract void OnMachineBuilt(BuildableMachineBase machineBase);
        public abstract void OnMachineRemoved(BuildableMachineBase machineBase);
    }
}