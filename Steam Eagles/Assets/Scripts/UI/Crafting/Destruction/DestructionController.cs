using System.Linq;
using System.Security.Cryptography;
using Buildables;
using Buildings;
using Items;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Zenject;

namespace UI.Crafting.Destruction
{
    public class DestructionPreviewController
    {
        public bool CanDestruct { get; private set; }

        private BuildableMachineBase _destructMachine;
        private BuildingCell[] _destructCells;

        [Inject]
        private DestructionHandlers _handlers;
        
        public void UpdateDestructTarget(Recipe recipe, PlayerInput playerInput, Building building, BuildingCell cell)
        {
            _handlers.UpdateDestructionTarget(playerInput, recipe, building, cell);
            // var bMachines = building.GetComponent<BMachines>();
            // BuildableMachineBase machineBase = bMachines.Map.GetMachine(cell.cell2D);
            // if (machineBase != null)
            // {
            //     _destructMachine = machineBase;
            //     _destructCells = null;
            //     CanDestruct = true;
            //     return;
            // }
            // _destructMachine = null;
            // var tile = building.Map.GetTileAnyLayer(cell,
            //     BuildingLayers.PIPE,
            //     BuildingLayers.SOLID,
            //     BuildingLayers.LADDERS)
            //     .ToArray();
            // _destructCells = tile;
            // if (tile.Length > 0)
            // {
            //     CanDestruct = true;
            //     return;
            // }
            //
            // CanDestruct = false;
        }
    }
}