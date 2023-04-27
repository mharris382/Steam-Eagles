using System;
using System.Collections.Generic;
using Buildings;
using Power.Steam;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildables
{
    public class BuildableMachine : BuildableMachineBase
    {
       
        [SerializeField]
        private Vector2Int cellSize = Vector2Int.one * new Vector2Int(3, 2);
        
        [SerializeField,TableList]
        private List<MachineCell> machineCells = new List<MachineCell>();
        
        [System.Serializable]
        public class MachineCell
        {
            [SerializeField]
            [Required] public TileBase tile;
            
            [HideInInspector]
            [SerializeField]
            public Vector2Int cellPosition;

            [ShowInInspector]
            public Vector2Int CellPosition
            {
                get => cellPosition;
                set
                {
                    #if UNITY_EDITOR
                    var machine = Selection.activeGameObject != null ? Selection.activeGameObject.GetComponent<BuildableMachine>() : null;
                    if (machine == null)
                        return;
                    var size = machine.MachineGridSize;
                    var x = Mathf.Min(value.x, size.x-1);
                    var y = Mathf.Min(value.y, size.y - 1);
                    cellPosition = new Vector2Int(x, y);
#endif
                }
            }
            public MachineCellType cellType;
            
            [ShowInInspector, ShowIf(nameof(HasConsumer))]
            private ConsumerNode _consumer;
            [ShowInInspector, ShowIf(nameof(HasSupplier))]
            private SupplierNode _supplier;

            [LabelText(nameof(amountLabel))]
            public float amount = 1000;
            public BuildingLayers targetLayer
            {
                get
                {
                    switch (cellType)
                    {
                        case MachineCellType.SteamInput:
                        case MachineCellType.SteamOutput:
                            return BuildingLayers.PIPE;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            public Color gizmoColor
            {
                get
                {
                    switch (cellType)
                    {
                        case MachineCellType.SteamInput:
                            return Color.blue;
                        case MachineCellType.SteamOutput:
                            return Color.red;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            string amountLabel => cellType == MachineCellType.SteamInput ? "Consumption Rate" : "Production Rate";

            public bool HasConsumer => _consumer != null;
            public bool HasSupplier => _supplier != null;



            public void OnMachineBuilt(Vector3Int position, Building building)
            {
                var steamNetwork = building.GetSteamNetwork();
                var cellPosition = (Vector3Int)this.CellPosition + position;
                switch (cellType)
                {
                    case MachineCellType.SteamInput:
                        _consumer = steamNetwork.AddConsumerAt(cellPosition);
                        _consumer.MaxConsumptionPerUpdate = _consumer.Capacity;
                        break;
                    case MachineCellType.SteamOutput:
                        _supplier = steamNetwork.AddSupplierAt(cellPosition);
                        _supplier.AmountSuppliedPerUpdate = _supplier.Capacity/2f;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }


        public enum MachineCellType
        {
            SteamInput,
            SteamOutput,
        }

        public Vector2Int CellSize => cellSize;

        public override Vector2Int MachineGridSize => cellSize;
        

        public Vector2 WsSize
        {
            get
            {
                if (GridLayout == null)
                {
                    return this.cellSize;
                }
                return (Vector2)GridLayout.cellSize * cellSize;
            }
        }


        private void OnDrawGizmos()
        {
            var building = GetComponentInParent<Building>();
            if (building == null) return;
            try
            {
                var position = this.CellPosition;
            }
            catch (Exception e)
            {
              return;
            }
            var size = this.MachineGridSize;
            foreach (var machineCell in machineCells)
            {
                var cellPos = machineCell.cellPosition + CellPosition;
                var worldPos = building.Map.CellToWorld((Vector3Int)cellPos, machineCell.targetLayer);
                var offset = building.Map.GetCellSize(machineCell.targetLayer);
                Gizmos.color = machineCell.gizmoColor;
                Gizmos.DrawCube(worldPos + (Vector3)offset / 2, offset);
            }
        }


        protected override void OnMachineBuilt(Vector3Int cell, Building building)
        {
            foreach (var machineCell in machineCells)
            {
                machineCell.OnMachineBuilt(cell, building);
            }
        }
    }
}
