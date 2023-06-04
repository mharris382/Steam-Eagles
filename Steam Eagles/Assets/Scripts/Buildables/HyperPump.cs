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
        public BuildableMachine BuildableMachine => _buildableMachine ? _buildableMachine : _buildableMachine = GetComponent<BuildableMachine>();
        public Building Building => BuildableMachine.Building;
        [Inject]
        public void Inject(HyperPumpController.Factory pumpControllerFactory)
        {
            _pumpController = pumpControllerFactory.Create(this);
        }
        public void Interact()
        {
            _pumpController.OnInteraction();
        }
        public Vector2Int GetOutputCell()
        {
            return producerCell.BuildingSpacePosition;
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