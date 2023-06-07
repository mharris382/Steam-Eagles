using System;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class HyperPump : MonoBehaviour
    {
        private HyperPumpController _pumpController;
        private BuildableMachine _buildableMachine;
        [Required] public MachineCell producerCell;
        private HypergasEngineConfig _config;
        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        public Building Building => BuildableMachine.Building;
        [Inject]
        public void Inject(HyperPumpController.Factory pumpControllerFactory, HypergasEngineConfig config)
        {
            _pumpController = pumpControllerFactory.Create(this);
            _config = config;
        }
        public void Interact()
        {
            _pumpController.OnInteraction();
        }
        public Vector2Int GetOutputCell()
        {
            return producerCell.BuildingSpacePosition;
        }

        [ShowInInspector, BoxGroup("Debugging"), ReadOnly,HideInEditorMode]
        public bool IsProducing
        {
            get;
            set;
        }
        [ShowInInspector, BoxGroup("Debugging"), ReadOnly,HideInEditorMode]
        public float ProductionRate
        {
            get;
            set;
        }

        [ShowInInspector, BoxGroup("Debugging"), ReadOnly, ProgressBar(0, "StorageCapacity"),HideInEditorMode]
        public float AmountStored
        {
            get;
            set;
        }
        
        
        float StorageCapacity => _config==null ? 0: _config.pumpStorageCapacity;

        private void OnDestroy()
        {
            _pumpController?.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (_pumpController == null)
            {
                return;
            }
            Gizmos.color = Color.red;
            var position = Building.Map.CellToWorld((Vector3Int)producerCell.BuildingSpacePosition, BuildingLayers.PIPE);
            var cellSize = Building.Map.GetCellSize(BuildingLayers.PIPE);
            var offset = cellSize / 2f;
            Gizmos.DrawCube(position + (Vector3)offset, cellSize);
        }
    }
}