using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CoreLib;
using GasSim.SimCore.DataStructures;
using JetBrains.Annotations;
using Unity.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace GasSim
{
    public interface IPressureGrid
    {
        void SetState(Vector2Int coord, StateOfMatter stateOfMatter);
        StateOfMatter GetState(Vector2Int coord);
    }

    [RequireComponent(typeof(ParticleSystem))]
    public class GasSimParticleSystem : MonoBehaviour, IGasSim
    {
        #region [INNER CLASSES]

       

        /// <summary>
        /// stores the state of the gas simulation on a grid
        /// </summary>
        internal class PressureGrid : IPressureGrid
        {
            private readonly byte[,] _grid;
            private readonly StateOfMatter[,] _stateGrid;
            internal  readonly GridHelper _gridHelper;
        
            [System.Obsolete("_usedCells instead")]
            private readonly BinaryHeap<Vector2Int> _nonEmptyPositions;
        
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
                    if(!_gridHelper.IsPositionOnGrid(coord))
                        throw new InvalidPressureGridOperation(coord);
                
                    return _grid[coord.x, coord.y];
                }
                set
                {
                    value = Mathf.Clamp(value, 0, 15);
                    //the reason I'm throwing an exception here because read/write access to this grid must be precise to ensure conservation of mass
                    if (!IsValidPressure(value) || !_gridHelper.IsPositionOnGrid(coord))
                        throw new InvalidPressureGridOperation(coord, value, $"Is PRESSURE VALID?{IsValidPressure(value)}\nIS POS VALID?{_gridHelper.IsPositionOnGrid(coord)}");
                
                
                
#if false
                var previousPressure = (byte)this[coord];
                var newPressure = (byte)Mathf.Clamp(value, 0, 15);

                //case 0: cell pressure did not change
                if (previousPressure == newPressure)
                {
                    return;
                }
                
                //case 1: previously empty cell became non-empty
                if (previousPressure == 0)
                {
                    _totalPressureInGrid += newPressure;
                    _nonEmptyPositions.Insert(coord, PressureToWeight(newPressure));
                }
                
                //case 2: previously non-empty cell became empty
                else if (newPressure == 0)
                {
                    _totalPressureInGrid -= previousPressure;
                    _nonEmptyPositions.Remove(coord);
                }
                
                else//case 3: cell pressure state, increased or decreased 
                {
                    _totalPressureInGrid += newPressure - previousPressure;
                    _nonEmptyPositions.ChangeKey(coord, PressureToWeight(newPressure));
                }
#else
                    var previousPressure = (byte)this[coord];
                    var newPressure = (byte)Mathf.Clamp(value, 0, 15);

                    if (PressureIsUnchanged())
                    {
                        return;
                    }

                
                    //cannot put non-zero gas into a solid block
                    if (newPressure != 0 && _stateGrid[coord.x, coord.y] == StateOfMatter.SOLID)
                        return;
                
                    if (CellWasEmpty())
                    {
                        _totalPressureInGrid += newPressure;
                        if (!_usedCells.ContainsKey(coord))//_nonEmptyPositions.Contains(coord))
                        {
                            _usedCells.Add(coord, PressureToWeight(newPressure));
                            //_nonEmptyPositions.Insert(coord, PressureToWeight(newPressure));
                        }
                        else
                        {
                            _usedCells[coord] = newPressure;
                            // _nonEmptyPositions.ChangeKey(coord, PressureToWeight(newPressure));
                        }
                    }
                    else if (CellIsEmpty())
                    {
                        _totalPressureInGrid -= previousPressure;
                        if (_usedCells.ContainsKey(coord)) _usedCells.Remove(coord);
                    }
                    else
                    {
                        _totalPressureInGrid += newPressure - previousPressure;
                        _usedCells[coord] = PressureToWeight(newPressure);
                        //_nonEmptyPositions.ChangeKey(coord, PressureToWeight(newPressure));
                    }
                
                    bool PressureIsUnchanged() => previousPressure == newPressure;
                    bool CellWasEmpty() => previousPressure == 0;
                    bool CellIsEmpty() => newPressure == 0;
#endif
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
                var heapCapacity = sizeX * sizeY;
                Debug.Log($"Number of Cells on Grid = {(heapCapacity).ToString().Bolded()}");
                _nonEmptyPositions = new BinaryHeap<Vector2Int>();
                _nonEmptyPositions.StartHeap(heapCapacity);
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
        
            private float PressureToWeight(int pressure)
            {
                return pressure / 16f;
            }

            private bool IsValidPressure(int pressure) => pressure is >= 0 and < 16;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="mode">0 for in order, 1 for depth first, 2 for breadth first</param>
            public void PrintHeap(int mode = 0)
            {
                StringBuilder sb = new StringBuilder();

                mode = Mathf.Clamp(mode, 0, 2);
                (IEnumerable<(Vector2Int, int)>,string)enumerableAndLine = mode switch
                {
                    0 => (_nonEmptyPositions.InOrderWithDepth(), "InOrder"),
                    1 => (_nonEmptyPositions.DepthFirstWithDepth(), "DepthFirst"),
                    2 => (_nonEmptyPositions.BreadthFirstWithDepth(), "BreadthFirst"),
                    _ => (null, "ERROR")
                };
                IEnumerable<(Vector2Int,int)> enumerable = enumerableAndLine.Item1;
                sb.AppendLine(enumerableAndLine.Item2.Bolded());
            
                if (enumerable != null)
                {
                    foreach (var pos in enumerable)
                    {
                        for (int i = 0; i < pos.Item2; i++) sb.Append('\t');
                    
                        Vector2Int coord = pos.Item1;
                        sb.AppendFormat("{0}:{1}", pos.Item1.ToString(), this[coord]);
                        sb.AppendLine();
                    }
                }
                Debug.Log(sb.ToString());
            }

            public void ClearGrid()
            {
                if (UsedCellsCount == 0) return;
                foreach (var cell in _nonEmptyPositions.DepthFirst())
                {
                    _grid[cell.x, cell.y] = 0;
                }
                _totalPressureInGrid = 0;
                _nonEmptyPositions.Clear();
            }

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
        
            public struct CellChange
            {
                public Vector2Int coord;
                public int newPressure;

                public override int GetHashCode()
                {
                    return coord.GetHashCode();
                }
            }

            public int GetMaxTransferAmount(Vector2Int from, Vector2Int to, int amount)
            {
                int current = this[from];
                int available = GetAvailableSpaceInCell(to);
                int maxTransferAmount = Mathf.Min(amount, 15);
                return Mathf.Min(current, maxTransferAmount, available);
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
    

        /// <summary>
        /// provides implementations for common grid operations and queries.  Has no knowledge of the gas simulation.
        /// TODO: this needs to be turned into a component so it can be used by any class which wants to interface with the gas simulation.  
        /// </summary>
        internal class GridHelper
        {
            private BoundsInt _bounds;
            private readonly Vector2Int[] _neighborDirs;
            public BoundsInt Bounds => _bounds;
            public RectInt Rect => new RectInt(Bounds.xMin, Bounds.yMin, Bounds.size.x, Bounds.size.y);
            public Vector2Int Clamp(Vector2Int position)
            {
                position.x = Mathf.Clamp(position.x,_bounds.min.x, _bounds.max.x);
                position.y = Mathf.Clamp(position.y,_bounds.min.y, _bounds.max.y);
                return position;
            }
        
            public Vector3Int Clamp(Vector3Int position)
            {
                position.x = Mathf.Clamp(position.x,_bounds.min.x, _bounds.max.x);
                position.y = Mathf.Clamp(position.y,_bounds.min.y, _bounds.max.y);
                return position;
            }

            public bool IsPositionOnGrid(Vector3Int position)
            {
                position.z = _bounds.min.z;
                return _bounds.Contains(position);
            }

            public bool IsPositionOnGrid(Vector2Int position) =>
                IsPositionOnGrid(new Vector3Int(position.x, position.y, 0));
        
        
        
            public GridHelper(GameObject owner, int sizeX, int sizeY)
            {
                const int BOUNDS_Z = 100;
                var _min = new Vector2Int(0, 0);
                _bounds = new BoundsInt(_min.x, _min.y, -BOUNDS_Z, sizeX, sizeY, BOUNDS_Z);
                _neighborDirs = new Vector2Int[4]
                {
                    Vector2Int.up, Vector2Int.right, Vector2Int.left, Vector2Int.down
                };
            }



            public IEnumerable<Vector2Int> GetNeighbors(Vector2Int cell)
            {
                //do this check once here since it will usually be true instead of same check 4 times   
                if (cell.x > _bounds.xMin+1 && cell.x < _bounds.xMax-1 &&
                    cell.y > _bounds.yMin+1 && cell.y < _bounds.yMax-1)
                {
                    return _neighborDirs.Select(t => t + cell);
                }

                return _neighborDirs.Select(t => t + cell).Where(IsPositionOnGrid);
            }
        
            public int GetNeighborCount(Vector2Int cell)
            {
                //do this check once here since it will usually be true instead of same check 4 times   
                if (cell.x > _bounds.xMin && cell.x < _bounds.xMax &&
                    cell.y > _bounds.yMin && cell.y < _bounds.yMax)
                {
                    return 4;
                }

                if (cell == (Vector2Int)_bounds.max ||
                    cell == (Vector2Int)_bounds.min ||
                    cell == new Vector2Int(_bounds.xMin, _bounds.yMax) ||
                    cell == new Vector2Int(_bounds.xMax, _bounds.yMin))
                {
                    return 2;
                }
                else
                {
                    return 3;
                }
            }

            public void DrawGizmos(float cellSizeX,float cellSizeY)
            {
                var rect = new Rect(_bounds.xMin*cellSizeX, _bounds.yMin*cellSizeX, _bounds.size.x*cellSizeY, _bounds.size.y*cellSizeY);
                rect.DrawGizmos();
            }
        }

    
    
        private class InvalidPressureGridOperation : InvalidOperationException
        {
            public InvalidPressureGridOperation(Vector2Int coord, int valueToSet) : base($"Invalid Pressure Operation Occured setting {valueToSet} at {coord}") { }
            public InvalidPressureGridOperation(Vector2Int coord, int valueToSet, string message) : base($"Invalid Pressure Operation Occured setting {valueToSet} at {coord}\n{message.InItalics()}") { }
            public InvalidPressureGridOperation(Vector2Int coord) : base($"Invalid Pressure Operation Occured getting value at {coord}") { }
        }

    
        #region [ENUM DECLARATIONS]

        private enum TestingMode
        {
            DISABLE_TESTS,
            RANDOM_PARTICLES,
            GRID_TO_PARTICLES,
            PARTICLES_TO_GRID,
            GRID_MOVEMENTS,
        }
        #endregion
    
        #endregion

    
   


        #region [FIELDS]

        [SerializeField] private IGasSim.GridResolution resolution =IGasSim.GridResolution.HALF;
        [SerializeField] internal PressureColor pressureColor;
        [SerializeField] private Vector2Int gridSize = new Vector2Int(100, 100);
        [SerializeField] private float pressureToLifetime = 0.125f;
        [Tooltip("Enable to offset world space positions by pressure")]
        [SerializeField] bool useZOffset = true;

        [SerializeField] private float pressureToZ = 1;
        [SerializeField] private float updateRate = 1;
        [SerializeField] private bool skipInternalGrid = true;
        [FormerlySerializedAs("copyGridToParticles")] [SerializeField] private bool enableGridToParticles = false;
        [SerializeField] private TestingMode testingMode;
        [SerializeField] private float particleSizeMultiplier = 2f;
        [SerializeField] private int randomSeed = 100;
        [SerializeField] private RectInt spawnRect = new RectInt(4, 4, 10, 10);

        [SerializeField] private bool runAsParallel = false;
        [Header("Sources")] [SerializeField,Range(0, 1)] private float sourceUpdateRate = 0.2f;
        [SerializeField] private GameObject[] sceneSources;
        private bool updateParticlesOnSourceUpdate = false;
        private Grid _cellGrid;
        private ParticleSystem _ps;
        private Vector2 _cellSize;
        private PressureGrid _pressureGrid;
        private GridHelper _gridHelper;
        
        private List<IGasSource> _registeredSources = new List<IGasSource>();
        private List<IGasSink> _registeredSinks = new List<IGasSink>();
    
        #endregion

        public void AddGasSourceToSimulation(IGasSource source)
        {
            if (!_registeredSources.Contains(source))
            {

                _registeredSources.Add(source);
            }
        }

        public void RemoveGasSourceFromSimulation(IGasSource source)
        {
            if (_registeredSources.Contains(source))
            {

                _registeredSources.Remove(source);
            }
        }

        public void AddGasSinkToSimulation(IGasSink sink)
        {
            if (!_registeredSinks.Contains(sink))
            {
                _registeredSinks.Add(sink);
            }
        }

        public void RemoveGasSinkFromSimulation(IGasSink source)
        {
            if (_registeredSinks.Contains(source))
            {
                _registeredSinks.Remove(source);
            }
        }

        #region [PROPERTIES]

        public Vector2 CellSize
        {
            get
            {
                if (_cellSize == Vector2.zero) SetGridSize();
                return _cellSize;
            }
        }
        private ParticleSystem ParticleSystem
        {
            get =>_ps == null ? (_ps = GetComponent<ParticleSystem>()) : _ps;
        }

        public Grid Grid
        {
            get
            {
                if (_cellGrid == null)
                {
                    if (!gameObject.TryGetComponent(out _cellGrid))
                    {
                        _cellGrid = gameObject.AddComponent<Grid>();
                        _cellGrid.cellSize = CellSize;
                    }
                }

                _cellGrid.cellSize = CellSize;
                return _cellGrid;
            }
        }

        internal PressureGrid InternalPressureGrid => _pressureGrid ??= new PressureGrid(gameObject,gridSize.x, gridSize.y);

        private GridHelper gridHelper => _gridHelper ??= new GridHelper(gameObject, gridSize.x, gridSize.y);

        public RectInt SimulationRect => gridHelper.Rect;
    

        #endregion

        #region [PRIVATE METHODS]

        private void SetGridSize()
        {
            _cellSize = resolution switch
            {
                IGasSim.GridResolution.FULL => Vector2.one,
                IGasSim.GridResolution.HALF => Vector2.one / 2f,
                IGasSim.GridResolution.QUART => Vector2.one / 4f,
                IGasSim.GridResolution.EIGHTH => Vector2.one / 8f,
                IGasSim.GridResolution.X16 => Vector2.one / 16f,
                IGasSim.GridResolution.X32 => Vector2.one / 32f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


    
        /// <summary>
        /// just adds a rect of particles to the particle system 
        /// </summary>
        private void SetupInitialGasState()
        {
        
            int spawnX = spawnRect.xMin;  int spawnY = spawnRect.yMin;
            int sizeX = spawnRect.width; int sizeY = spawnRect.height;
            bool skipInternalUpdate = this.skipInternalGrid;
            int count = sizeX * sizeY;
            ParticleSystem.Clear(false);
            this.ParticleSystem.Emit(count);
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
            int index = 0;
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                
                    int pressure = Random.Range(0, 8) + Random.Range(4,8);
                    pressure *= (Random.value > 0.4f) ? 1 : 0;
                    Vector2Int cellCoord = new Vector2Int(x+spawnX, y+spawnY);
                
                    Vector3 size3D = ParticleGetSize3D();
                    float size = size3D.x;
                    float lifetime = 1;
                    if (!skipInternalUpdate)
                    {
                        if (InternalPressureGrid.GetState(cellCoord) == StateOfMatter.SOLID)
                        {
                            lifetime = 0;
                            pressure = 0;
                        }
                        else
                        {
                            InternalPressureGrid[cellCoord] = pressure;
                        }
                    }
                
                    uint seed = (uint)ParticleGetRandomSeed(cellCoord);
                    try
                    {
                        particles[index] = new ParticleSystem.Particle()
                        {
                            startColor = ParticleGetColor(pressure),
                            position = ParticleGetPosition(cellCoord, pressure),
                            startLifetime = lifetime,
                            remainingLifetime = lifetime,
                            randomSeed = seed,
                            startSize3D = size3D,
                            startSize = size,
                            axisOfRotation = Vector3.forward,
                            angularVelocity = 0,
                            velocity = Vector3.zero
                        };

                    }
                    catch (IndexOutOfRangeException e)
                    {
                        Debug.LogWarning($"IndexOutOfRangeException Occured At: {index} pos: ({x},{y})\nCount={count}\n\n {e.StackTrace}");
                    
                        break;
                    }
                    catch (InvalidPressureGridOperation e)
                    {
                        throw;
                    }
                    if(!skipInternalUpdate)
                        InternalPressureGrid[cellCoord] = pressure;
                    index++;
                }
            }
            ParticleSystem.SetParticles(particles);
            Debug.Log($"Particle System Particle Count = {count}, Grid used cells = {InternalPressureGrid.UsedCellsCount}");
        }

        #region [Particle Methods]

        private int ParticleGetRandomSeed(Vector2Int cellCoord)
        {
            return this.randomSeed + ((cellCoord.x * randomSeed)* (cellCoord.y * randomSeed));
        }

        private Vector2 ParticleGetSize3D()
        {
            return CellSize * particleSizeMultiplier;
        }

        private Vector3 ParticleGetPosition(Vector2Int cellCoord, int pressure)
        {
            return useZOffset ? CellToWorldSpace(cellCoord, pressure) : CellToWorldSpace(cellCoord);
        }

        private Color ParticleGetColor(int pressure)
        {
            return pressureColor.PressureToColor(pressure);
        }
        private Vector3 ParticleGetVelocity(Vector2Int cellCoord, int pressure)
        {
            return Vector3.zero;
        }
        private float ParticleGetStartLifetime(int gasCellPressure)
        {
            //return enableGridToParticles ? updateRate : gasCellPressure * pressureToLifetime;
            return updateRate + 0.1f;
        }

        #endregion

        private void UpdateParticlesFromGrid()
        {
        
            int targetCount = this.InternalPressureGrid.UsedCellsCount;

        
            this.ParticleSystem.Clear(false);
            this.ParticleSystem.Emit(targetCount);
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[targetCount];
            int i = 0;
            foreach (var gasCell in InternalPressureGrid.GetAllNonEmptyCells())
            {
                var startLifetime = ParticleGetStartLifetime(gasCell.pressure);
                particles[i] = new ParticleSystem.Particle()
                {
                    position = ParticleGetPosition(gasCell.coord, gasCell.pressure),
                    startColor = ParticleGetColor(gasCell.pressure),
                    startLifetime = startLifetime,
                    remainingLifetime =startLifetime,
                    velocity = ParticleGetVelocity(gasCell.coord, gasCell.pressure),
                    randomSeed = (uint)ParticleGetRandomSeed(gasCell.coord),
                    startSize3D = ParticleGetSize3D(),
                    angularVelocity = 0,
                    axisOfRotation = Vector3.forward
                };
                i++;
            }
            //
            ParticleSystem.SetParticles(particles);
        }

    
        private void UpdateGridFromParticles()
        {
            NativeArray<ParticleSystem.Particle> particles =
                new NativeArray<ParticleSystem.Particle>(ParticleSystem.particleCount, Allocator.Temp);
            int count = ParticleSystem.GetParticles(particles);
            Dictionary<Vector2Int, int> _amountAdded = new Dictionary<Vector2Int, int>();
            foreach (var particle in particles)
            {
                var coord = WorldToCell(particle.position);
                int pressureInParticle = pressureColor.ColorToPressure(particle.GetCurrentColor(ParticleSystem));
                int pressureInGrid = InternalPressureGrid[coord];
                if (!_amountAdded.TryAdd(coord, pressureInGrid))//the grid already 
                {
                
                }
            }
            particles.Dispose();
        }

    

        private Vector3 CellToWorldSpace(Vector2Int cell)
        {
            var  pos = Grid.CellToWorld(new Vector3Int(cell.x, cell.y));
        
            return pos;
        }
        private Vector3 CellToWorldSpace(Vector2Int cell, int pressure)
        {
            var  pos = Grid.CellToWorld(new Vector3Int(cell.x, cell.y));
            pos.z = (pressure / 16f)*pressureToZ;
            return pos;
        }
        private Vector2Int WorldToCell(Vector3 world) => (Vector2Int)Grid.WorldToCell(world);

        #endregion

        #region [UNITY MESSAGES]

        private void Awake()
        {
            this.TimerStart();

            _ps = GetComponent<ParticleSystem>();
            _pressureGrid = new PressureGrid(gameObject, gridSize.x, gridSize.y);
            SetGridSize();
            Grid.cellSize = CellSize;
        
            var childSources = GetComponentsInChildren<IGasSource>();
            _registeredSources.AddRange(childSources);
        
            foreach (var sceneSource in sceneSources.Where(t=>t!=null))
            {
                childSources = sceneSource.GetComponentsInChildren<IGasSource>();
                _registeredSources.AddRange(childSources);
            }

            this.TimerStop("Awake");
        }

        private void Start()
        {
            this.TimerStart();
        
            //SetupInitialGasState();
            this.TimerPrintout("Setup Initial Gas State");
        
            int count = this.ParticleSystem.particleCount;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
            ParticleSystem.GetParticles(particles);
        
            this.TimerStop("Start");
        
            Invoke(nameof(UpdateSim), updateRate);
            InvokeRepeating(nameof(DoGridIO), 1, sourceUpdateRate);
        }

    
    
    
        void UpdateSim()
        {
            switch (testingMode)
            {
                case TestingMode.DISABLE_TESTS:
                    break;
                case TestingMode.RANDOM_PARTICLES:
                    SetupInitialGasState();
                    break;
                case TestingMode.GRID_TO_PARTICLES:
                    UpdateParticlesFromGrid();
                    break;
                case TestingMode.PARTICLES_TO_GRID:
                    UpdateGridFromParticles();
                    Invoke(nameof(UpdateSimStep2), updateRate/2f);
                    return;
                case TestingMode.GRID_MOVEMENTS:
                    DoGridMovements();
                    UpdateParticlesFromGrid();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Invoke(nameof(UpdateSim), updateRate);
        }
    


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            InternalPressureGrid._gridHelper.DrawGizmos(CellSize.x, CellSize.y);
        }

        #endregion

        private void DoGridMovements()
        {
            bool ChooseRandom() => Random.Range(1, 5) <= 2;
            var nonEmpty = InternalPressureGrid.GetAllNonEmptyCells().ToArray();
            try
            {
                foreach (var cell in nonEmpty)
                {

                    Vector2Int from, to;
                    var emptyNeighbors = InternalPressureGrid.GetEmptyNeighbors(cell.coord).ToArray();
                    if (emptyNeighbors.Length > 0)
                    {
                        from = cell.coord;
                        to = ChooseRandom() ? emptyNeighbors[Random.Range(0, emptyNeighbors.Length)] : emptyNeighbors[0];
                        InternalPressureGrid.Transfer(from, to, 1);
                        continue;
                    }

                    var validNeighbors = InternalPressureGrid.GetLowerDensityNeighbors(cell.coord).ToArray();
                    if (validNeighbors.Length > 0)
                    {
                        from = cell.coord;
                        to = ChooseRandom() ? validNeighbors[Random.Range(0, validNeighbors.Length)] : validNeighbors[0];
                        InternalPressureGrid.Transfer(from, to, 1);
                        continue;
                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        private void DoGridIO()
        {
            if (_registeredSources.Count == 0) 
                return;
            
            if (runAsParallel)
            {
                foreach (var sourceCell in _registeredSources.SelectMany(t => t.GetSourceCells().AsParallel()))
                {
                    TryAddGasToCell(sourceCell.coord, sourceCell.amount);
                }
            }
            else
            {
                foreach (var sourceCell in _registeredSources.SelectMany(t => t.GetSourceCells()))
                {
                    TryAddGasToCell(sourceCell.coord, sourceCell.amount);
                }
                foreach (var sinkCells in _registeredSinks.SelectMany(t => t.GetSourceCells()))
                {
                    TryRemoveGasFromCell(sinkCells.coord, sinkCells.amount);
                }

            }

            if (updateParticlesOnSourceUpdate)
            {
                CancelInvoke(nameof(UpdateSim));
                UpdateSim();
            }
        }
        void UpdateSimStep2()
        {
            UpdateParticlesFromGrid();
            Invoke(nameof(UpdateSim), updateRate/2f);
        }
        public void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter)
        {
            if (gridHelper.IsPositionOnGrid(coord) == false)
            {
                Debug.LogError($"Coordinate: {coord} is not on grid!");
                return;
            }

            var size = CellSize;
            Vector2 pos = CellToWorldSpace(coord);
            Rect rect = new Rect(pos - size / 2f, size);
            Vector3[] pnts = new Vector3[5]
            {
                new Vector3(rect.xMin, rect.yMin),
                new Vector3(rect.xMin, rect.yMax),
                new Vector3(rect.xMax, rect.yMax),
                new Vector3(rect.xMax, rect.yMin),
                new Vector3(rect.xMin, rect.yMin)
            };
            for (int i = 1; i < pnts.Length; i++)
            {
                var p0 = pnts[i - 1];
                var p1 = pnts[i];
                Debug.DrawLine(p0, p1, Color.magenta, 2);
            }
            InternalPressureGrid.SetState(coord, stateOfMatter);
        }

        public int GetPressureThatCanBeAdded(Vector2Int coord)
        {
            return 15 - InternalPressureGrid[coord];
        }
        public bool CanAddGasToCell(Vector2Int coord, ref int amount)
        {
            return Mathf.Min(amount, GetPressureThatCanBeAdded(coord)) > 0;
        }
        public bool TryAddGasToCell(Vector2Int coord, int amount)
        {
            if (CanAddGasToCell(coord, ref amount))
            {
                InternalPressureGrid[coord] += amount;
                return true;
            }

            return false;
        }
        public bool CanRemoveGasFromCell(Vector2Int coord, ref int amount)
        {
            return Mathf.Max(amount, InternalPressureGrid[coord]) > 0;
        }
        public bool TryRemoveGasFromCell(Vector2Int coord, int amount)
        {
            if (CanRemoveGasFromCell(coord, ref amount))
            {
                InternalPressureGrid[coord] -= amount;
                return true;
            }

            return false;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GasSimParticleSystem))]
    public class GasSimParticleSystemEditor : Editor
    {
        private bool expanded = false;

        public override void OnInspectorGUI()
        {
            var gasSim = target as GasSimParticleSystem;
            var pressureColor = gasSim.pressureColor;
            if (EditorGUILayout.Foldout(expanded, "Pressure to Color"))
            {
                GUILayout.BeginVertical();
                EditorGUI.indentLevel++;
                for (int i = 0; i < 16; i++)
                {
                    GUILayout.BeginHorizontal();

                    const float spacePressureIn = 32f; const float spaceColor = 0.4f; const float spacePressureOut = 0.2f;
                    int pressureIn = i;
                    EditorGUILayout.LabelField(pressureIn.ToString(), GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            
                    var color = pressureColor.PressureToColor(i);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ColorField(color, GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
                    EditorGUI.EndDisabledGroup();
                    int pressureOut = pressureColor.ColorToPressure(color);
                    EditorGUILayout.LabelField(pressureOut.ToString(), GUILayout.MaxWidth(spacePressureIn), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(false));
            
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);
        
        
            if (Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("In Order"))
                {
                    gasSim.InternalPressureGrid.PrintHeap(0);
                }
                if (GUILayout.Button("Depth Order"))
                {
                    gasSim.InternalPressureGrid.PrintHeap(1);
                }
                if (GUILayout.Button("Breast Order"))
                {
                    gasSim.InternalPressureGrid.PrintHeap(2);
                }
                GUILayout.EndHorizontal();
            }
       
        
            base.OnInspectorGUI();
        }
    }

#endif
}