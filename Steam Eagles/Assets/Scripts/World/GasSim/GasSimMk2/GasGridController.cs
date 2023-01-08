using System;
using UnityEngine;

namespace GasSim
{
    [RequireComponent(typeof(Grid))]
    public class GasGridController : MonoBehaviour
    {
        [SerializeField] private GridResolution resolution = GridResolution.HALF;
        [SerializeField] private Vector2Int chunkSize = new Vector2Int(16, 16);

        private GasSimGrid _gasSimGrid;
        private Grid _grid;
        private RectInt _simulationBounds;
        private GasSimParticleSystem.GridHelper _gridHelper;

        public Vector2 CellSize => Grid.cellSize;
        public Grid Grid => _grid ? _grid : _grid = GetAndSetupGrid();

        public GasSimGrid GasSimGrid => _gasSimGrid ??= new GasSimGrid(chunkSize);

        private void Awake()
        {
            _gasSimGrid = new GasSimGrid(chunkSize);
            _grid = GetAndSetupGrid();
        }

        private Grid GetAndSetupGrid()
        {
            var grid = GetComponent<Grid>();
            grid.cellSize = resolution switch
            {
                GridResolution.FULL => Vector2.one,
                GridResolution.HALF => Vector2.one / 2f,
                GridResolution.QUART => Vector2.one / 4f,
                GridResolution.EIGHTH => Vector2.one / 8f,
                GridResolution.X16 => Vector2.one / 16f,
                GridResolution.X32 => Vector2.one / 32f,
                _ => throw new ArgumentOutOfRangeException()
            };
            return grid;
        }


        public void ResizeSimulation(Vector2Int position, Vector2Int size)
        {
            _simulationBounds = new RectInt(position, size);
            _gasSimGrid.ResizeSimulation(size);
            _gridHelper = new GasSimParticleSystem.GridHelper(_simulationBounds);
        }

        public Vector2Int WorldToSimPosition(Vector2 worldSpaceCoord)
        {
            var localPosition = (Vector2Int)WorldToGridPosition((Vector3)worldSpaceCoord);
            return localPosition - _simulationBounds.position;
        }
        
        public Vector2 SimToWorldPosition(Vector2Int simCoordinate)
        {
            var gridPosition = SimToGridPosition(simCoordinate);
            return Grid.CellToWorld((Vector3Int)gridPosition);
        }
        
        
        public bool IsInSimulationBounds(Vector2Int simCoordinate)
        {
            return _simulationBounds.Contains(simCoordinate);
        }
        
        public bool IsInSimulationBounds(Vector2 wsPos)
        {
            return false;
        }
        
        public Vector2 GridToWorldPosition(Vector2Int gridCoordinate) => Grid.CellToWorld((Vector3Int)gridCoordinate);

        public Vector2Int WorldToGridPosition(Vector3 worldSpacePosition) => (Vector2Int)Grid.WorldToCell(worldSpacePosition);

        public Vector2Int GridToSimPosition(Vector2Int gridCoord)
        {
            return gridCoord - _simulationBounds.position;
        }
        public Vector2Int SimToGridPosition(Vector2Int simCoord)
        {
            return simCoord + _simulationBounds.position;
        }
    }
}