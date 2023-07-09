using Buildables.Interfaces;
using Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables.Utilities
{
    public class TileSetter : MonoBehaviour, IMachineListener
    {
        public BuildingLayers layers = BuildingLayers.PIPE;
        public TileBase tileBase;

        public void OnMachineBuilt(BuildableMachineBase machineBase)
        {
            var building = machineBase.Building;
            var pos = building.Map.WorldToCell(transform.position, layers);
            building.Map.SetTile(new BuildingCell(pos, layers),tileBase);
        }

        public void OnMachineRemoved(BuildableMachineBase machineBase)
        {
            var building = machineBase.Building;
            var pos = building.Map.WorldToCell(transform.position, layers);
            building.Map.SetTile(new BuildingCell(pos, layers), null);
        }
    }
}