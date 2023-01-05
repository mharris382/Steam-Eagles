using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreLib;
using GasSim.SimCore.DataStructures;
using UnityEngine;

namespace GasSim
{
    public partial class GasSimParticleSystem
    {
        /// <summary>
        /// stores the state of the gas simulation on a grid
        /// </summary>
        internal class PressureGrid : IPressureGrid
        {
            private readonly byte[,] _grid;
            private readonly ParticleVelocity[,] _velocities;
            private readonly StateOfMatter[,] _stateGrid;
            internal  readonly GridHelper _gridHelper;
           
        
            private Dictionary<Vector2Int, float> _usedCells = new Dictionary<Vector2Int, float>();
        
            private int _totalPressureInGrid;
            public int TotalPressureInGrid => _totalPressureInGrid;
            public int NumberOfCellsUsed => _usedCells.Count;
        
            public float TotalCellDensity
            {
                get
                {
                    if (NumberOfCellsUsed == 0) return 0f;
                    var totalPressureInGrid = TotalPressureInGrid / (float)NumberOfCellsUsed;
                    return totalPressureInGrid;
                }
            }

            public int this[Vector2Int coord]
            {
                get
                {
                    //the reason I'm throwing an exception here because access to this grid must be precise to ensure conservation of mass
                    if (!_gridHelper.IsPositionOnGrid(coord))
                        return 0;
                
                    return _grid[coord.x, coord.y];
                }
                set
                {
                    value = Mathf.Clamp(value, 0, 15);
                    
                    if (!_gridHelper.IsPositionOnGrid(coord) || (GetState(coord) == StateOfMatter.SOLID && value > 0))
                        return;

                    var previousPressure = (byte)this[coord];
                    var newPressure = (byte)value;
                    //case 0: cell pressure did not change
                    if (previousPressure == newPressure)
                    {
                        return;
                    }
                  
                    //case 1: previously empty cell became non-empty
                    if (previousPressure == 0)
                    {
                        _totalPressureInGrid += newPressure;
                        _usedCells.Add(coord, newPressure);
                    }
                    //case 2: previously non-empty cell became empty
                    else if (newPressure == 0)
                    {
                        _totalPressureInGrid -= previousPressure;
                        _usedCells.Remove(coord);
                    }
                    else//case 3: cell pressure state, increased or decreased 
                    {
                        _totalPressureInGrid += newPressure - previousPressure;
                        _usedCells[coord] = newPressure;
                    }
                    _grid[coord.x, coord.y] = newPressure;
                }
            }

            public void SetState(Vector2Int coord, StateOfMatter stateOfMatter)
            {
                _stateGrid[coord.x, coord.y] = stateOfMatter;
                if (stateOfMatter == StateOfMatter.SOLID)
                {
                    this[coord] = 0;
                }
            }

            public StateOfMatter GetState(Vector2Int coord)
            {
                return _stateGrid[coord.x, coord.y];
            }
            
            
            public PressureGrid(GameObject owner, int sizeX, int sizeY)
            {
                const int BOUNDS_Z = 100;
                _grid = new byte[sizeX, sizeY];
                _stateGrid = new StateOfMatter[sizeX, sizeY];
                _gridHelper = new GridHelper(owner, sizeX, sizeY);
                _velocities = new ParticleVelocity[sizeX, sizeY];                
            }

            public int UsedCellsCount => _usedCells.Count;

            public IEnumerable<(Vector2Int coord, int pressure)> GetAllNonEmptyCells()
            {
#if USE_HEAP
            foreach (var cell in _nonEmptyPositions.DepthFirst())
            {
                yield return (cell, this[cell]);
            }
#else
                foreach (var usedCell in _usedCells)
                {
                    yield return (usedCell.Key, this[usedCell.Key]);
                }
#endif
            }
        
            private float PressureToWeight(int pressure) => pressure / 16f;

            private bool IsValidPressure(int pressure) => pressure is >= 0 and < 16;


            public IEnumerable<Vector2Int> GetEmptyNeighbors(Vector2Int cell)
            {
                return _gridHelper.GetNeighbors(cell).Where(t => this[t] == 0 && _stateGrid[t.x, t.y] == StateOfMatter.AIR);
            }

            public int GetAvailableSpaceInCell(Vector2Int cell)
            {
                return 16 - this[cell];
            }
         
            public IEnumerable<Vector2Int> GetLowerDensityNeighbors(Vector2Int cell)
            {
                int pressure = this[cell];
                return _gridHelper.GetNeighbors(cell).Where(t => this[t] < pressure && _stateGrid[t.x, t.y] == StateOfMatter.AIR);
            }

            public int GetMaxTransferAmount(Vector2Int from, Vector2Int to, int amount)
            {
                int current = this[from];
                int available = GetAvailableSpaceInCell(to);
                int maxTransferAmount = Mathf.Min(amount, 15);
                return Mathf.Min(current, maxTransferAmount, available);
            }



            public void SetTransferVelocity(Vector2Int from, Vector2Int to, int amount)
            {
                var diff = to - from;
                diff.x *= amount;
                diff.y *= amount;
                _velocities[to.x, to.y] = new ParticleVelocity(diff);
            }

            public void Transfer(Vector2Int from, Vector2Int to, int amount)
            {
                amount = GetMaxTransferAmount(from, to, amount);
                int f0 = this[from];
                int f1 = f0 - amount;
                int t0 = this[to];
                int t1 = t0 + amount;
                this[from] = f1;
                this[to] = t1;
            }
            
        }
    }


    public struct ParticleVelocity
    {
        
        private byte _x;
        private byte _y;
        private byte _amount;

        public ParticleVelocity(byte x, byte y, byte amount)
        {
            _x = x;
            _y = y;
            _amount = amount;
        }
        
        public ParticleVelocity(Vector2Int velocity, int amount = 1)
        {
            _x = (byte)velocity.x;
            _y = (byte)velocity.y;
            _amount = (byte)amount;
        }
        public Vector2Int Velocity
        {
            get
            {
                return new Vector2Int(_x, _y);
            }
            set
            {
                _x = (byte)value.x;
                _y = (byte)value.y;
            }
        }
                
        public int VelocityX
        {
            get
            {
                return _x;
            }
            set
            {
                _x = (byte)value;
            }
        }
                
        public int VelocityY
        {
            get
            {
                return _y;
            }
            set
            {
                _y = (byte)value;
            }
        }
    }
}