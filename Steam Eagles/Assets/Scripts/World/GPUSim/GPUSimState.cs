using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace GasSim.GPUSim
{
  
    public class GPUSimState : IGasSim
    {
        private BoundsInt _cellBounds;
        private Bounds _localBounds;
        private Grid _grid;
        private readonly Camera _environmentCam;
        private readonly BoxCollider2D _environmentBounds;
        private readonly RenderTexture _environmentTexture;
        
        
        private byte[,] _cellDensity;
        private byte[,] _cellVelocity;
        private StateOfMatter [,] _cellState;
        
        private int _cellCountX;
        private int _cellCountY;
        


        public GPUSimState(BoxCollider2D environmentBounds, Vector2 cellSize)
        {
            this._environmentBounds = environmentBounds;
            var go = new GameObject("GPU Gas Sim");
            _grid = go.AddComponent<Grid>();
            _grid.cellSize = cellSize;
            var cameraGO = new GameObject("GPU Gas Sim Camera");
            go.transform.SetParent(environmentBounds.transform);
            go.transform.localPosition = Vector3.zero;
            
            cameraGO.transform.SetParent(go.transform);
            cameraGO.transform.localPosition = -Vector3.forward*10;
            
            _environmentCam = cameraGO.AddComponent<Camera>();
            _environmentCam.orthographic = true;
            _environmentCam.backgroundColor = Color.white;
            _environmentCam.cullingMask = LayerMask.GetMask("Solids");
            
            var simSizeWS = environmentBounds.size;
            var simSizeCS = new Vector2Int(Mathf.CeilToInt(simSizeWS.x / cellSize.x), Mathf.CeilToInt(simSizeWS.y / cellSize.y));
            simSizeCS.x = simSizeCS.y = Mathf.Max(simSizeCS.x, simSizeCS.y);
            
            Debug.Log("Created gas simulation with size " + simSizeCS);
            
            _environmentCam.orthographicSize = simSizeWS.y / 2f;
            
            var simSolidRT = new RenderTexture(simSizeCS.x, simSizeCS.y, 0);
            simSolidRT.enableRandomWrite = true;
            simSolidRT.Create();
            
            _environmentCam.targetTexture = simSolidRT;
            _environmentTexture = simSolidRT;
            
            _cellCountX = simSizeCS.x;
            _cellCountY = simSizeCS.y;
            SimulationRect = new RectInt(0, 0, _cellCountX, _cellCountY);
        }

        public void Init()
        {

            _environmentBounds.OnTriggerEnter2DAsObservable()
                .Select(t => t.GetComponent<ISimAgent>())
                .Where(a => a != null)
                .Subscribe(AddAgentToSim)
                .AddTo(_environmentBounds);
            
            _environmentBounds.OnTriggerExit2DAsObservable()
                .Select(t => t.GetComponent<ISimAgent>())
                .Where(t => t != null)
                .Subscribe(RemoveAgentFromSim)
                .AddTo(_environmentBounds);

            _environmentCam.Render();
            _cellDensity = new byte[_cellCountX, _cellCountY];
            _cellVelocity = new byte[_cellCountX, _cellCountY];
            _cellState = new StateOfMatter[_cellCountX, _cellCountY];
        }

        void AddAgentToSim(ISimAgent agent)
        {
            
        }

        void RemoveAgentFromSim(ISimAgent agent)
        {
            
        }


        public Grid Grid => _grid;
        public RectInt SimulationRect { get; }
        public void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter)
        {
            Vector2Int simCoord;
            if (GetSimCoordFromGrid(coord, out simCoord))
            {
                _cellState[simCoord.x, simCoord.y] = stateOfMatter;
            }
        }

        private bool GetSimCoordFromGrid(Vector2Int coord, out Vector2Int simCoord)
        {
            Vector3Int cellCoord = new Vector3Int(coord.x, coord.y, 0);
            simCoord = Vector2Int.zero;
            if (!_cellBounds.Contains(cellCoord))
            {
                return false;
            }
            throw new System.NotImplementedException();
        }

        public bool TryAddGasToCell(Vector2Int coord, int amount)
        {
            throw new System.NotImplementedException();
        }

        public bool TryRemoveGasFromCell(Vector2Int coord, int amount)
        {
            throw new System.NotImplementedException();
        }

        public bool CanAddGasToCell(Vector2Int coord, ref int amount)
        {
            throw new System.NotImplementedException();
        }

        public bool CanRemoveGasFromCell(Vector2Int coord, ref int amount)
        {
            throw new System.NotImplementedException();
        }
    }
}