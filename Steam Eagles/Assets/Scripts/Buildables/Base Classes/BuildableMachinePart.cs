using System;
using Buildings;
using UnityEngine;

namespace Buildables
{
    public abstract class BuildableMachinePart : MonoBehaviour
    {
        private BuildableMachineBase _buildableMachine;
        private SpriteRenderer _sr;

        public Color gizmoColor = Color.green;
        
        public SpriteRenderer sr => _sr != null ? _sr : _sr = GetComponent<SpriteRenderer>();

        public BuildableMachineBase BuildableMachine => _buildableMachine != null ? _buildableMachine : _buildableMachine = GetComponentInParent<BuildableMachineBase>();

        public GridLayout Grid => HasResources()? BuildableMachine.GridLayout : null;

        public bool HasResources() => BuildableMachine != null && BuildableMachine.HasResources();


        protected Vector3Int GetCell(Building building) => building.Map.WorldToCell(transform.position, Layer);
        
        protected abstract BuildingLayers Layer { get; }

        public abstract void OnBuild(Building building);

        
        private void OnDrawGizmos()
        {
            if (Grid == null) return;
            Gizmos.color = gizmoColor;
            var position = transform.position;
            var cellPos = Grid.WorldToCell(position);
            var cellCenter = Grid.CellToWorld(cellPos);
            var cellSize = Grid.cellSize;
            Gizmos.DrawWireCube(cellCenter, cellSize);
        }
    }
}