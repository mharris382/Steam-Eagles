using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;


namespace GasSim
{
    [RequireComponent(typeof(Grid), typeof(GasSimVisualizer), typeof(SimIORegistry))]
    public class GasSimulator : MonoBehaviour
    {
        public const int PRESSURE_MAX = 15;
        
        private Grid _grid;
        private GasSimVisualizer _visualizer;
        private SimIORegistry _simIORegistry;
        
        private Dictionary<Vector2Int, int> _usedCells = new Dictionary<Vector2Int, int>();
        private StateOfMatter[,] _stateOfMatter;
        private List<Vector2Int> _toBeRemoved = new List<Vector2Int>();
        private BoundsInt _simBounds;


        [SerializeField] private BoxCollider2D boundingArea;
        [MinMaxRange(1, 50)] public Vector2Int randomChance = new Vector2Int(2, 5);
        public float updateRate = 1f;
        [SerializeField] private bool autoPositionGrid = true;

        Grid Grid => _grid ? _grid : _grid = GetComponent<Grid>();
        private SimIORegistry simIORegistry => _simIORegistry ? _simIORegistry : _simIORegistry = GetComponent<SimIORegistry>();
        GasSimVisualizer Visualizer => _visualizer ? _visualizer : _visualizer = GetComponent<GasSimVisualizer>();

        
        
        private Vector2Int[] _neighbors = new Vector2Int[4];
        private Vector2Int[] _neighborDirections;
        private Vector2Int[] _neighborDirections2;
        private int _solidLayerMask;

        private void Awake()
        {
            _grid = GetComponent<Grid>();
            _neighborDirections = new Vector2Int[4] { Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down };
            _neighborDirections2 = new Vector2Int[4] { Vector2Int.up, Vector2Int.left, Vector2Int.right, Vector2Int.down };
            
            var worldSpaceBounds =
                new Bounds(boundingArea.transform.TransformPoint(boundingArea.offset), boundingArea.size);
            var cellSize = Grid.cellSize;
            var cellCountX = Mathf.CeilToInt(worldSpaceBounds.size.x / cellSize.x);
            var cellCountY = Mathf.CeilToInt(worldSpaceBounds.size.y / cellSize.y);
            transform.parent = boundingArea.transform;
            if(autoPositionGrid)
                transform.position = worldSpaceBounds.min;

            _simBounds = new BoundsInt(Vector3Int.zero, new Vector3Int(cellCountX, cellCountY, 1));
            _simIORegistry = GetComponent<SimIORegistry>();
            _visualizer = GetComponent<GasSimVisualizer>();
            simIORegistry.InitializeIOTracking(boundingArea, _grid, _simBounds);


            this._solidLayerMask = LayerMask.GetMask("Solids", "Ground");
            _stateOfMatter = new StateOfMatter[cellCountX, cellCountY];
            for (int x = 0; x < cellCountX; x++)
            {
                for (int y = 0; y < cellCountY; y++)
                {
                    _stateOfMatter[x, y] = Physics2D.OverlapPoint(Grid.GetCellCenterWorld(new Vector3Int(x, y, 0)), _solidLayerMask) ?
                        StateOfMatter.SOLID : StateOfMatter.AIR;
                }
            }

            MessageBroker.Default.Receive<BuildActionInfo>().DelayFrame(1).Where(t => t.tilemapType == TilemapTypes.SOLIDS).Subscribe(OnBuildTile).AddTo(this);
            MessageBroker.Default.Receive<DisconnectActionInfo>().DelayFrame(1).Where(t => t.tilemapType == TilemapTypes.SOLIDS).Subscribe(OnDisconnectTile).AddTo(this);
        }

        private void OnEnable()
        {
            StartCoroutine(nameof(Simulate));
        }

        private void OnDisable()
        {
            StopCoroutine(nameof(Simulate));
        }
        
        
        #region [SOLID TILEMAP CHANGED EVENTS]

        private void SolidTilemapUpdated(Vector3Int cellPos, Tilemap tilemap, Vector3 worldPos)
        {
            if (_simBounds.Contains(cellPos))
            {
                var xCells = Mathf.CeilToInt((tilemap.cellSize.x * tilemap.transform.lossyScale.x) / Grid.cellSize.x) + 2;
                var yCells = Mathf.CeilToInt((tilemap.cellSize.y * tilemap.transform.lossyScale.y) / Grid.cellSize.y) + 2;
                var x = cellPos.x;
                var y = cellPos.y;
                for (int i = 0; i < xCells; i++)
                {
                    for (int j = 0; j < yCells; j++)
                    {
                        var cell = new Vector3Int(x + i, y + j);
                        if (_simBounds.Contains(cell))
                        {
                            var cellWorldPos = Grid.GetCellCenterWorld(cell);
                            if (Physics2D.OverlapPoint(cellWorldPos, _solidLayerMask))
                            {
                                _stateOfMatter[cell.x, cell.y] = StateOfMatter.SOLID;
                                _usedCells.Remove(new Vector2Int(cell.x, cell.y));
                            }
                            else
                            {
                                _stateOfMatter[cell.x, cell.y] = StateOfMatter.AIR;
                            }

                            Debug.DrawLine(worldPos, cellWorldPos,
                                _stateOfMatter[cell.x, cell.y] == StateOfMatter.AIR ? Color.cyan : Color.red, 1);
                        }
                    }
                }
            }
        }

        void OnBuildTile(BuildActionInfo buildActionInfo)
        {
            var worldPos = buildActionInfo.worldPosition;
            var cellPos = Grid.WorldToCell(worldPos);
            var tilemap = buildActionInfo.tilemap;
            cellPos.z = 0;
            SolidTilemapUpdated(cellPos, tilemap, worldPos);
        }

        void OnDisconnectTile(DisconnectActionInfo disconnectActionInfo)
        {
            var worldPos = disconnectActionInfo.WorldPosition;
            var cellPos = Grid.WorldToCell(worldPos);
            cellPos.z = 0;
            SolidTilemapUpdated(cellPos, disconnectActionInfo.tilemap, worldPos);
        }
        

        #endregion


        #region [HELPER METHODS]

        int GetRemainingGasCapacity(Vector2Int simCoord)
        {
            if (_usedCells.ContainsKey(simCoord))
            {
                return PRESSURE_MAX - _usedCells[simCoord];
            }
            return PRESSURE_MAX;
        }
        int GetGas(Vector2Int simCoord)
        {
            if (_usedCells.ContainsKey(simCoord))
            {
                return _usedCells[simCoord];
            }
            return 0;
        }
        bool TryAddGas(Vector2Int simCoord, ref int amt)
        {
            if (_stateOfMatter[simCoord.x, simCoord.y] == StateOfMatter.AIR)
            {
                if (_usedCells.ContainsKey(simCoord))
                {
                    amt = Mathf.Max(amt, GetRemainingGasCapacity(simCoord));
                    _usedCells[simCoord] += amt;
                }
                else
                {
                    _usedCells.Add(simCoord, amt);
                }
                return true;
            }
            return false;
        }
        bool TryRemoveGas(Vector2Int simCoord, ref int amt)
        {
            if (_stateOfMatter[simCoord.x, simCoord.y] == StateOfMatter.AIR)
            {
                int amountCanRemove = GetGas(simCoord);
                if (amountCanRemove > 0)
                {
                    amt = Mathf.Min(amt, amountCanRemove);
                    _usedCells[simCoord] -= amt;
                    if(_usedCells[simCoord]<=0) _usedCells.Remove(simCoord);
                    return true;
                }
                return false;
            }
            return false;
        }

        #endregion

        IEnumerator Simulate()
        {
            while (true)
            {
                SimulateSinks();
                SimulateSources();
                Diffuse();
                Visualizer.UpdateParticlesFromGridData(_usedCells, updateRate);
                yield return new WaitForSeconds(updateRate);
            }
        }

        private void SimulateSinks()
        {
            //remove pressure from cells at sink locations
            foreach (var sink in simIORegistry.GetSimIOSinks())
            {
                var cellPos = sink.cellSpacePos;
                var amount = sink.amount;
                if (TryRemoveGas(cellPos, ref amount))
                {
                    sink.onGasRemoved?.Invoke(amount);                    
                }
            }
        }

        private void SimulateSources()
        {
            // try to add pressure to a cell at source locations
            foreach (var source in simIORegistry.GetSimIOSources())
            {
                var cellPos = new Vector3Int(source.cellSpacePos.x, source.cellSpacePos.y, 0);
                var amount = source.amount;
                if (_simBounds.Contains(cellPos) && TryAddGas(source.cellSpacePos, ref amount))
                {
                    source.onGasAdded?.Invoke(amount);
                }
            }
        }


        private void Diffuse()
        {
            Vector2Int[] neighbors = new Vector2Int[4];
            bool ChooseRandom() => Random.Range(0, randomChance.y) > randomChance.x;
            int GetAmountToTransfer(int pressureDiff) => Mathf.Max(1, Mathf.FloorToInt(pressureDiff / 2f));
            var usedCells = _usedCells.ToArray();
            // spread gas to neighboring cells with lower pressure
            foreach (var cell in usedCells)
            {
                Vector2Int fromCell = cell.Key;
                Vector2Int toCell;
                int fromPressure = cell.Value;
                int pressureDiff;
                int emptyNeighborCount = GetEmptyNeighbors(fromCell, out neighbors);
                if (emptyNeighborCount > 0)
                {
                    pressureDiff = fromPressure;
                    int amountToTransfer = GetAmountToTransfer(pressureDiff);
                    toCell = ChooseRandom() ? neighbors[Random.Range(0, emptyNeighborCount)] : neighbors[0];
                    TransferGas(fromCell, toCell, amountToTransfer);
                    continue;
                }

                int lowerPressureNeighborCount = GetLowerDensityNeighbors(fromCell, out neighbors);
                if (lowerPressureNeighborCount > 0)
                {
                    toCell = ChooseRandom() ? neighbors[Random.Range(0, lowerPressureNeighborCount)] : neighbors[0];
                    pressureDiff = fromPressure - _usedCells[toCell];
                    int amountToTransfer = GetAmountToTransfer(pressureDiff);
                    TransferGas(fromCell, toCell, amountToTransfer);
                    continue;
                }
            }

            foreach (var cell in _toBeRemoved)
            {
                _usedCells.Remove(cell);
            }
            _toBeRemoved.Clear();
        }

        void TransferGas(Vector2Int from, Vector2Int to, int amt)
        {
            if (!_usedCells.ContainsKey(to))
            {
                _usedCells.Add(to, 0);
            }
            int pressureInFromCell = _usedCells[from];
            int pressureInToCell = _usedCells[to];
            amt = Mathf.Min(amt, pressureInFromCell, PRESSURE_MAX-pressureInToCell);
            _usedCells[to] += amt;
            _usedCells[from] -= amt;
            Debug.Assert(!(_usedCells[from] < 0));
            if(_usedCells[from] <= 0)
            {
                _usedCells.Remove(from);
            }
        }
        
        int GetEmptyNeighbors(Vector2Int coord, out Vector2Int[] neighbors)
        {
            var neighborDirections = Random.value > 0.5f ? _neighborDirections : _neighborDirections2;
           
            var emptyNeighbors = 0;
            for (int i = 0; i < neighborDirections.Length; i++)
            {
                var neighbor = coord + neighborDirections[i];
                if (_simBounds.Contains(new Vector3Int(neighbor.x, neighbor.y, 0))
                    && !_usedCells.ContainsKey(neighbor)
                    && _stateOfMatter[neighbor.x, neighbor.y] == StateOfMatter.AIR)
                {
                    _neighbors[emptyNeighbors] = neighbor;
                    emptyNeighbors++;
                }
            }
            neighbors = _neighbors;
            return emptyNeighbors;
        }
        
        int GetLowerDensityNeighbors(Vector2Int coord, out Vector2Int[] neighbors)
        {
            var neighborDirections = Random.value > 0.5f ? _neighborDirections : _neighborDirections2;
            var lowerDensityNeighbors = 0;
            var cellDensity = _usedCells[coord];
            for (int i = 0; i < neighborDirections.Length; i++)
            {
                var neighbor = coord + neighborDirections[i];
                if (_simBounds.Contains(new Vector3Int(neighbor.x, neighbor.y, 0))
                    && _stateOfMatter[neighbor.x, neighbor.y] == StateOfMatter.AIR
                    && (!_usedCells.ContainsKey(neighbor) || _usedCells[neighbor] < cellDensity))
                {
                    _neighbors[lowerDensityNeighbors] = neighbor;
                    lowerDensityNeighbors++;
                }
            }
            neighbors = _neighbors;
            return lowerDensityNeighbors;
        }




    #region Drawing Functions

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            DrawSimIOPointsOnGrid(simIORegistry.dynamicSources);
            if (!Application.isPlaying) return;
            var minCell = _simBounds.min;
            var maxCell = _simBounds.max;
            var points = new Vector3Int[]
            {
                new Vector3Int(minCell.x, minCell.y),
                new Vector3Int(maxCell.x, minCell.y),
                new Vector3Int(maxCell.x, maxCell.y),
                new Vector3Int(minCell.x, maxCell.y),
                new Vector3Int(minCell.x, minCell.y)
            };
            for (int i = 1; i < points.Length; i++)
            {
                var cellP0 = points[i-1];
                var cellP1 = points[i];
                var worldP0 = Grid.CellToWorld(cellP0);
                var worldP1 = Grid.CellToWorld(cellP1);
                Gizmos.DrawLine(worldP0, worldP1);
            }
        }
        
        private void DrawSimIOPointsOnGrid(List<SimIOPoint> simIOPoints)
        {
            foreach (var point in simIOPoints)
            {
                DrawPointOnCell(point.transform.position);
            }
        }

        private void DrawPointOnCell(Vector3 worldPosition)
        {
            var cellSpace = Grid.WorldToCell(worldPosition);
            var cellToWorld = Grid.GetCellCenterWorld(cellSpace);
            Gizmos.DrawCube(cellToWorld, Vector3.one);
            //cell = cellSpace.ToString();
        }

        #endregion
        
    }
} 