using System;
using Buildables.Interfaces;
using Buildings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables.Utilities
{
    public class TileSetter : TileUtility
    {
       [ValidateInput(nameof(ValidateLayer))] public BuildingLayers layers = BuildingLayers.PIPE;
       [ValidateInput(nameof(ValidateTile))]public TileBase tileBase;
       
       
       private Subject<BuildableMachineBase> _onMachineBaseBuilt = new Subject<BuildableMachineBase>();
         private Subject<BuildableMachineBase> _onMachineBaseRemoved = new Subject<BuildableMachineBase>();
         
         
         public IObservable<BuildableMachineBase> OnMachineBaseBuilt => _onMachineBaseBuilt;
            public IObservable<BuildableMachineBase> OnMachineBaseRemoved => _onMachineBaseRemoved;

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
            _onMachineBaseBuilt.OnNext(machineBase);
        }

        public override void OnMachineRemoved(BuildableMachineBase machineBase)
        {
            UnSetTile(machineBase);
            _onMachineBaseRemoved.OnNext(machineBase);
        }


        public void SetTile(BuildableMachineBase machineBase=null)
        {
            var c = Cell;
            c.ClearZ();
            Debug.Assert(c.cell.z == 0);
            (machineBase ? machineBase : Machine).Building.Map.SetTile(c,tileBase);
        }

        public void UnSetTile(BuildableMachineBase machineBase=null)
        {
            var c = Cell;
            c.ClearZ();
            Debug.Assert(c.cell.z == 0);
            (machineBase ? machineBase : Machine).Building.Map.SetTile(c,null);
        }
    }
}