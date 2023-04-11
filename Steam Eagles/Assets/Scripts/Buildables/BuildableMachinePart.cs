using System;
using UnityEngine;

namespace Buildables
{
    public abstract class BuildableMachinePart : MonoBehaviour
    {
        private BuildableMachineBase _buildableMachine;
        public BuildableMachineBase BuildableMachine => _buildableMachine != null ? _buildableMachine : _buildableMachine = GetComponentInParent<BuildableMachineBase>();

        public GridLayout Grid => HasResources()? BuildableMachine.GridLayout : null;

        public bool HasResources() => BuildableMachine != null && BuildableMachine.HasResources();

        public Color gizmoColor = Color.green;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            var position = transform.position;
            var cellPos = Grid.WorldToCell(position);
            var cellCenter = Grid.CellToWorld(cellPos);
            var cellSize = Grid.cellSize;
            Gizmos.DrawWireCube(cellCenter, cellSize);
        }
    }
}