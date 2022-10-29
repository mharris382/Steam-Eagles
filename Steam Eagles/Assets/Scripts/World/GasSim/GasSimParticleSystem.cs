using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CoreLib;
using GasSim.SimCore.DataStructures;
using Unity.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Rand = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;

#endif
[RequireComponent(typeof(ParticleSystem))]
public class GasSimParticleSystem : MonoBehaviour
{
    #region [INNER CLASSES]

    [Serializable]
    internal class PressureColor
    {
        [Range(0,1)]
        [SerializeField] private float alphaMinPressure = 0.2f; 
        [Range(0,1)]
        [SerializeField] private float alphaMaxPressure = 0.8f; 
        
        
        public Color PressureToColor(int pressure, Color current)
        {
            current.a = PressureToAlpha(pressure);
            return current;
        }
        public Color PressureToColor(int pressure)
        {
            var current = Color.white;
            current.a = PressureToAlpha(pressure);
            return current;
        }

        public int ColorToPressure(Color color)
        {
            return AlphaToPressure(color.a);
        }

        private float PressureToAlpha(int pressure)
        {
            if (pressure <= 0) return 0f;
            int mult = (int)Mathf.Sign(pressure);
            float t = pressure / 16f;
            return Mathf.Lerp(alphaMinPressure, alphaMaxPressure, t) *  mult;
        }

        private int AlphaToPressure(float alpha)
        {
            int mult = (int)Mathf.Sign(alpha);

            int p = Mathf.RoundToInt(Mathf.InverseLerp(alphaMinPressure, alphaMaxPressure, alpha) * 16);
            return p * mult;
        }
    }


    internal class PressureGrid
    {
        private readonly byte[,] _grid;
        private readonly GridHelper _gridHelper;
        private readonly BinaryHeap<Vector2Int> _nonEmptyPositions;
        private int _totalPressureInGrid;
        public int TotalPressureInGrid => _totalPressureInGrid;
        public int NumberOfCellsUsed => _nonEmptyPositions.Count;
        
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
                if (CellWasEmpty())
                {
                    _totalPressureInGrid += newPressure;
                    _nonEmptyPositions.Insert(coord, PressureToWeight(newPressure));
                }
                else if (CellIsEmpty())
                {
                    _totalPressureInGrid -= previousPressure;
                    _nonEmptyPositions.Remove(coord);
                }
                else
                {
                    _totalPressureInGrid += newPressure - previousPressure;
                    _nonEmptyPositions.ChangeKey(coord, PressureToWeight(newPressure));
                }
                
                bool PressureIsUnchanged() => previousPressure == newPressure;
                bool CellWasEmpty() => previousPressure == 0;
                bool CellIsEmpty() => newPressure == 0;
#endif
                _grid[coord.x, coord.y] = newPressure;
            }
        }
        
        
        public PressureGrid(int sizeX, int sizeY)
        {
            const int BOUNDS_Z = 100;
            _grid = new byte[sizeX, sizeY];
            _gridHelper = new GridHelper(sizeX, sizeY);
            var heapCapacity = sizeX * sizeY;
            Debug.Log($"Number of Cells on Grid = {(heapCapacity).ToString().Bolded()}");
            _nonEmptyPositions = new BinaryHeap<Vector2Int>();
            _nonEmptyPositions.StartHeap(heapCapacity);
        }

        public int UsedCellsCount => _nonEmptyPositions.Count;

        public IEnumerable<(Vector2Int coord, int pressure)> GetAllNonEmptyCells()
        {
            foreach (var cell in _nonEmptyPositions.DepthFirst())
            {
                yield return (cell, this[cell]);
            }
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
        private class InvalidPressureGridOperation : InvalidOperationException
        {
            public InvalidPressureGridOperation(Vector2Int coord, int valueToSet) : base($"Invalid Pressure Operation Occured setting {valueToSet} at {coord}") { }
            public InvalidPressureGridOperation(Vector2Int coord, int valueToSet, string message) : base($"Invalid Pressure Operation Occured setting {valueToSet} at {coord}\n{message.InItalics()}") { }
            public InvalidPressureGridOperation(Vector2Int coord) : base($"Invalid Pressure Operation Occured getting value at {coord}") { }
        }
    }
    
    private class GridHelper
    {
        private BoundsInt _bounds;
        
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
        public GridHelper(int sizeX, int sizeY)
        {
            const int BOUNDS_Z = 100;
            var _min = new Vector2Int(0, 0);
            _bounds = new BoundsInt(_min.x, _min.y, -BOUNDS_Z, sizeX, sizeY, BOUNDS_Z);
        }
    }

    #endregion

    
    #region [ENUM DECLARATIONS]


    private enum GridResolution
    {
        FULL,
        HALF,
        QUART,
        EIGHTH,
        X16,
        X32
    }

    #endregion


    #region [FIELDS]

    [SerializeField] private GridResolution resolution =GridResolution.HALF;
    [SerializeField] internal PressureColor pressureColor;
    [SerializeField] private Vector2Int gridSize = new Vector2Int(100, 100);
    [SerializeField] private float pressureToLifetime = 0.125f;
    [Tooltip("Enable to offset world space positions by pressure")]
    [SerializeField] bool useZOffset = true;

    [SerializeField] private float pressureToZ = 1;
    [SerializeField] private bool skipInternalGrid = true;
    [SerializeField] private float particleSizeMultiplier = 2f;
    [SerializeField] private int randomSeed = 100;
    [SerializeField] private RectInt spawnRect = new RectInt(4, 4, 10, 10);
    private Grid _cellGrid;
    private ParticleSystem _ps;
    private Vector2 _cellSize;
    private PressureGrid _pressureGrid;
    
    #endregion

    
    #region [PROPERTIES]

    private Vector2 CellSize
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
    private Grid Grid
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

    internal PressureGrid InternalPressureGrid => _pressureGrid ?? (_pressureGrid = new PressureGrid(gridSize.x, gridSize.y));

    #endregion

    #region [PRIVATE METHODS]

    private void SetGridSize()
    {
        _cellSize = resolution switch
        {
            GridResolution.FULL => Vector2.one,
            GridResolution.HALF => Vector2.one / 2f,
            GridResolution.QUART => Vector2.one / 4f,
            GridResolution.EIGHTH => Vector2.one / 8f,
            GridResolution.X16 => Vector2.one / 16f,
            GridResolution.X32 => Vector2.one / 32f,
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
        this.ParticleSystem.Emit(count);
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        int index = 0;
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                
                int pressure = Rand.Range(0, 8) + Rand.Range(4,8);
                pressure *= (Rand.value > 0.4f) ? 1 : 0;
                Vector2Int cellCoord = new Vector2Int(x+spawnX, y+spawnY);
                
                Vector3 size3D = ParticleGetSize3D();
                float size = size3D.x;
                
                uint seed = (uint)ParticleGetRandomSeed(cellCoord);
                try
                {
                    particles[index] = new ParticleSystem.Particle()
                    {
                        startColor = ParticleGetColor(pressure),
                        position = ParticleGetPosition(cellCoord, pressure),
                        startLifetime = 1,
                        remainingLifetime = 1,
                        randomSeed = seed,
                        startSize3D = size3D,
                        startSize = size,
                        axisOfRotation = Vector3.forward,
                        angularVelocity = 0,
                        velocity = Vector3.zero
                    };
                    if(!skipInternalUpdate)
                        InternalPressureGrid[cellCoord] = pressure;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogWarning($"IndexOutOfRangeException Occured At: {index} pos: ({x},{y})\nCount={count}");
                    break;
                }
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
        return gasCellPressure * pressureToLifetime;
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
        // ParticleSystem.SetParticles(particles);
    }

    
    private void UpdateGridFromParticles()
    {
        NativeArray<ParticleSystem.Particle> particles =
            new NativeArray<ParticleSystem.Particle>(ParticleSystem.particleCount, Allocator.Temp);
        
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

    #endregion

    #region [UNITY MESSAGES]

    private void Awake()
    {
        this.TimerStart();

        _ps = GetComponent<ParticleSystem>();
        _pressureGrid = new PressureGrid(gridSize.x, gridSize.y);
        SetGridSize();
        Grid.cellSize = CellSize;
       
       this.TimerStop("Awake");
    }

    private void Start()
    {
        this.TimerStart();
        
        SetupInitialGasState();
        this.TimerPrintout("Setup Initial Gas State");
        
        int count = this.ParticleSystem.particleCount;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        ParticleSystem.GetParticles(particles);
        
        this.TimerStop("Start");
        
        InvokeRepeating(nameof(UpdateParticlesFromGrid), 1, 1);
    }

    IEnumerator GasSim()
    {
        while (enabled)
        {
            SetupInitialGasState();
            yield return new WaitForSeconds(1);
        }
    }

    private void Update()
    {
        NativeArray<ParticleSystem.Particle> arrayParticles =
            new NativeArray<ParticleSystem.Particle>(ParticleSystem.particleCount, Allocator.Temp);

        int cnt = ParticleSystem.GetParticles(arrayParticles);

        
        
        arrayParticles.Dispose();
    }

    private void OnParticleUpdateJobScheduled()
    {
        Debug.Log("OnParticleUpdateJobScheduled".Bolded());
    }

    #endregion


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

