using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.VFX;

namespace Buildings
{
    [RequireComponent(typeof(Grid))]
    public class BuildingGrid : MonoBehaviour
    {
        
        [InlineButton(nameof(ComputeGridBounds))]
        public BoundsInt gridBounds;
        [InlineButton(nameof(UpdateVisualEffect))]
        public VisualEffect gasEffect;
        
        private Grid _grid;
        private Grid grid => _grid ? _grid : _grid = GetComponent<Grid>();

        public RenderTexture renderTexture;
        public Camera renderCamera;

        public BoundsInt[] tmBounds;

        void ComputeGridBounds()
        {
            var tilemaps = GetComponentsInChildren<Tilemap>();
            tmBounds = new BoundsInt[tilemaps.Length];
            foreach (var tilemap in tilemaps)
            {
                
                tilemap.CompressBounds();
                var bounds = tilemap.cellBounds;
                tmBounds[tilemap.transform.GetSiblingIndex()] = bounds;
            }

            var xMin = tmBounds.Min(t => t.xMin + (int)t.center.x);
            var yMin = tmBounds.Min(t => t.yMin + (int)t.center.y);
            var xMax = tmBounds.Max(t => t.xMax + (int)t.center.x);
            var yMax = tmBounds.Max(t => t.yMax + (int)t.center.y);
            
            var min = new Vector3Int(xMin, yMin,0);
            var max = new Vector3Int(xMax, yMax, 0);
            
            var wsMin = grid.CellToWorld(min);
            var wsMax = grid.CellToWorld(max);
            var wsSize = wsMax - wsMin;
            var wsCenter = (wsMax + wsMin) / 2f;
            
            gasEffect.SetVector3("Bounds Center WS", wsCenter);
            gasEffect.SetVector3("Bounds Size WS", wsSize);
            
            
        }

        
        void UpdateVisualEffect()
        {
            ComputeGridBounds();
            var cellBounds = this.gridBounds;
            
            var wsMin = grid.CellToWorld(cellBounds.min);
            var wsMax = grid.CellToWorld(cellBounds.max);
            var lsMin = grid.CellToLocal(cellBounds.min);
            var lsMax = grid.CellToLocal(cellBounds.max);
            var localBounds = new Bounds((lsMin + lsMax) / 2, lsMax - lsMin);
            var worldBounds = new Bounds((wsMin + wsMax) / 2, wsMax - wsMin);
            
            gasEffect.SetVector3("Bounds Center WS", worldBounds.center);
            gasEffect.SetVector3("Bounds Size WS", worldBounds.size);
            
        }
        
        void UpdateRTCam()
        {
            
        }
    }
}