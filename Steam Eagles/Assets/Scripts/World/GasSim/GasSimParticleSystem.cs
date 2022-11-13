using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CoreLib;
using JetBrains.Annotations;
using UniRx;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using World;
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
    public partial class GasSimParticleSystem : MonoBehaviour, IGasSim
    {
        #region [INNER CLASSES]

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
        [SerializeField]private bool updateParticlesOnSourceUpdate = false;
        private Grid _cellGrid;
        private ParticleSystem _ps;
        private Vector2 _cellSize;
        private PressureGrid _pressureGrid;
        private GridHelper _gridHelper;
        
        private List<IGasSource> _registeredSources = new List<IGasSource>();
        private List<IGasSink> _registeredSinks = new List<IGasSink>();
    
        #endregion

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

        #region [UNITY MESSAGES]

        private void Awake()
        {
            this.TimerStart();

            _ps = GetComponent<ParticleSystem>();
            _pressureGrid = new PressureGrid(gameObject, gridSize.x, gridSize.y);
            SetGridSize();
            Grid.cellSize = CellSize;
        
          
            MessageBroker.Default.Receive<IGasSource>().Where(t => _registeredSources.Contains(t)).TakeUntilDestroy(this).Subscribe(RemoveGasSourceFromSimulation);
            MessageBroker.Default.Receive<IGasSource>().Where(t => !_registeredSources.Contains(t)).TakeUntilDestroy(this).Subscribe(AddGasSourceToSimulation);
            MessageBroker.Default.Receive<IGasSink>().Where(t => _registeredSinks.Contains(t)).TakeUntilDestroy(this).Subscribe(RemoveGasSinkFromSimulation);
            MessageBroker.Default.Receive<IGasSink>().Where(t => !_registeredSinks.Contains(t)).TakeUntilDestroy(this).Subscribe(AddGasSinkToSimulation);
            
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
          
            Invoke(nameof(DoSimulationStep1), updateRate);
            InvokeRepeating(nameof(DoGridIO), 1, sourceUpdateRate);
        }

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

        #region [Gas Sources and Sinks]
        
        Dictionary<IGasSource, int> _gasTakenFromSources = new Dictionary<IGasSource, int>();
        Dictionary<IGasSink, int> _gasAddedToSinks = new Dictionary<IGasSink, int>();

        private void DoGridIO()
        {
            //this.TimerStart();
            
            
           // this.TimerPrintout("Starting Grid Sources (single thread, no parallel)");
            
            DoGridSources();
            
           // this.TimerPrintout("Processed Grid Sources");
            
            ApplySourceChangesToGrid();
            
           // this.TimerPrintout("Starting Grid Sinks");
            
            DoGridSinks();
            
            //this.TimerPrintout("Processed Grid Sinks");
            
            ApplySinkChangesToGrid();

            //this.TimerStop("Finished Grid IO (single thread, no parallel)");
            
            

            if (updateParticlesOnSourceUpdate)
            {
                CancelInvoke(lastSimulationStep == 1 ? nameof(DoSimulationStep2) : nameof(DoSimulationStep1));
                DoSimulationStep1();
            }
        }
        
       

        #region [Sources]
        
        /// <summary>
        /// simulation notifies the source how much gas was taken this update 
        /// </summary>
        private void ApplySourceChangesToGrid()
        {
            foreach (var source in _gasTakenFromSources) 
                source.Key.GasTakenFromSource(source.Value);
        }
        
        /// <summary>
        /// simulation queries the sources for gas changes
        /// </summary>
        private void DoGridSources()
        {
            foreach (var source in _registeredSources)
            {
                int totalAmount = 0;
                _gasTakenFromSources.TryAdd(source, 0);
                _gasTakenFromSources[source] = 0;
                foreach (var sourceCell in source.GetSourceCells())
                {
                    int amount = sourceCell.amount;
                    if (TryAddGasToCell(sourceCell.coord, ref amount))
                        totalAmount += amount;
                }

                if (totalAmount > 0)
                {
                    _gasTakenFromSources[source] = totalAmount;
                }
            }
        }

        /// <summary>
        /// Adds a sink to the simulation.  Each IO update, the simulation will query the source for the amount of gas it can take
        /// </summary>
        public void AddGasSourceToSimulation(IGasSource source)
        {
            if (!_registeredSources.Contains(source))
            {

                _registeredSources.Add(source);
            }
        }
        
        /// <summary>
        /// simulation will stop querying this source for gas
        /// </summary>
        public void RemoveGasSourceFromSimulation(IGasSource source)
        {
            if (_registeredSources.Contains(source))
            {
                _registeredSources.Remove(source);
            }
        }

        #endregion

        #region [Sinks]
        
        /// <summary> simulation transfers the gas amount that was removed from the simulation to the sink </summary>
        private void ApplySinkChangesToGrid()
        {
            foreach (var sink in _gasAddedToSinks)sink.Key.GasAddedToSink(sink.Value);
            _gasTakenFromSources.Clear();
        }
        
        /// <summary>
        /// simulation queries the sinks for gas changes
        /// <para>for safety reasons the computation is separated from the application of those changes</para>
        /// </summary>
        private void DoGridSinks()
        {
            foreach (var registeredSink in _registeredSinks)
            {
                int totalAmount = 0;
                foreach (var sourceCell in registeredSink.GetSourceCells())
                {
                    int amount = sourceCell.amount;
                    if (TryRemoveGasFromCell(sourceCell.coord, ref amount))
                        totalAmount += amount;
                }
        
            }
        }
        
        
        /// <summary>
        /// Adds a sink to the simulation.  Each IO update, the simulation will query the sink for the cells it wants to take gas from.
        /// </summary>
        /// <param name="sink"></param>
        public void AddGasSinkToSimulation(IGasSink sink)
        {
            if (!_registeredSinks.Contains(sink))
            {
                _registeredSinks.Add(sink);
            }
        }

        /// <summary>
        /// simulation will stop querying this sink for gas
        /// </summary>
        /// <param name="source"></param>
        public void RemoveGasSinkFromSimulation(IGasSink source)
        {
            if (_registeredSinks.Contains(source))
            {
                _registeredSinks.Remove(source);
            }
        }
        
        #endregion

        #endregion


        private int lastSimulationStep = -1;

        void DoSimulationStep1()
        {
            lastSimulationStep = 1;
            DoGridMovements();
            Invoke(nameof(DoSimulationStep2), updateRate/2f);
        }

        void DoSimulationStep2()
        {
            lastSimulationStep = 2;
            UpdateParticlesFromGrid();
            Invoke(nameof(DoSimulationStep1), updateRate/2f);
        }


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
                    int pressureDiff = 0;

                    int GetAmountToTransfer()
                    {
                        return Mathf.Max(1, Mathf.FloorToInt(pressureDiff / 2f));
                    }
                    if (emptyNeighbors.Length > 0)
                    {
                        from = cell.coord;
                        to = ChooseRandom() ? emptyNeighbors[Random.Range(0, emptyNeighbors.Length)] : emptyNeighbors[0];
                        pressureDiff = cell.pressure;
                        InternalPressureGrid.Transfer(from, to, GetAmountToTransfer() );
                        continue;
                    }

                    var validNeighbors = InternalPressureGrid.GetLowerDensityNeighbors(cell.coord).OrderByDescending(t => InternalPressureGrid[t]).ToArray();
                    if (validNeighbors.Length > 0)
                    {
                        from = cell.coord;
                        to = ChooseRandom() ? validNeighbors[Random.Range(0, validNeighbors.Length)] : validNeighbors[0];
                        var pressureFrom = InternalPressureGrid[from];
                        var pressureTo = InternalPressureGrid[to];
                        pressureDiff = pressureFrom - pressureTo;
                        InternalPressureGrid.Transfer(from, to, GetAmountToTransfer());
                        continue;
                    }

                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }

        
        #region [DIRECT PRESSURE READ/WRITE]

        public void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter)
        {
            if (gridHelper.IsPositionOnGrid(coord) == false)
            {
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

        public bool CanAddGasToCell(Vector2Int coord, int amount) => CanAddGasToCell(coord, amount);
        
        public bool CanRemoveGasFromCell(Vector2Int coord, int amount) => CanRemoveGasFromCell(coord, amount);
        
        public bool TryAddGasToCell(Vector2Int coord, ref int amount)
        {
            if (CanAddGasToCell(coord, ref amount))
            {
                InternalPressureGrid[coord] += amount;
                return true;
            }

            return false;
        }
        public bool TryAddGasToCell(Vector2Int coord, int amount) => TryAddGasToCell(coord, ref amount);

        public bool TryRemoveGasFromCell(Vector2Int coord,int amount) => TryRemoveGasFromCell(coord, amount);

        public bool CanRemoveGasFromCell(Vector2Int coord, ref int amount)
        {
            amount = Mathf.Min(amount, InternalPressureGrid[coord]);
            return amount > 0;
        }
        
        
        public bool TryRemoveGasFromCell(Vector2Int coord, ref int amount)
        {
            if (CanRemoveGasFromCell(coord, ref amount))
            {
                InternalPressureGrid[coord] -= amount;
                return true;
            }

            return false;
        }

        #endregion

        
        #region [PARTICLE SYSTEM]

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
        

        #endregion


        #region [EDITOR]

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            if (!Application.isPlaying)
            {
                var size = gridSize;

                Vector2Int min = (Vector2Int)this.Grid.WorldToCell(transform.position);
                var max = size + min;

                var maxWS = this.Grid.CellToWorld((Vector3Int)max);
                var minWS = this.Grid.CellToWorld((Vector3Int)min);
                Rect rect = new Rect(minWS, maxWS - minWS);
                rect.DrawGizmos();
                return;
            }
            InternalPressureGrid._gridHelper.DrawGizmos(CellSize.x, CellSize.y);
        }

        #endregion
    }
}