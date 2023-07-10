using Buildables.Interfaces;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables.Utilities
{
    public class TileSetter : TileUtility
    {
       [ValidateInput(nameof(ValidateTile))] public BuildingLayers layers = BuildingLayers.PIPE;
       [ValidateInput(nameof(ValidateLayer))]public TileBase tileBase;

        bool ValidateTile(TileBase tileBase) => Validate(layers, tileBase);
        bool ValidateLayer(BuildingLayers layer) => Validate(layer, tileBase);
        
        public override BuildingLayers TargetLayer => layers;

        protected virtual bool Validate(BuildingLayers layer, TileBase tile)
        {
            return layer != BuildingLayers.NONE && tile != null;
        }

        public override void OnMachineBuilt(BuildableMachineBase machineBase)
        {
            SetTile(machineBase);
        }

        public override void OnMachineRemoved(BuildableMachineBase machineBase)
        {
            UnSetTile(machineBase);
        }


        public void SetTile(BuildableMachineBase machineBase=null)
        {
            (machineBase ? machineBase : Machine).Building.Map.SetTile(Cell,tileBase);
        }

        public void UnSetTile(BuildableMachineBase machineBase=null)
        {
            (machineBase ? machineBase : Machine).Building.Map.SetTile(Cell,null);
        }
    }
}