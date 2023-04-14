using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.BuildingTilemaps
{
    [RequireComponent(typeof(Tilemap))]
    public class CellDebugger : MonoBehaviour
    {
        private Tilemap _tilemap;
        public Color color = Color.red;
        public Vector3Int cellPosition;
        
        
        
        void OnDrawGizmos()
        {
            if (_tilemap == null)
            {
                _tilemap = GetComponent<Tilemap>();
            }
            var cellWorldPosition = _tilemap.GetCellCenterWorld(cellPosition);
            Gizmos.color = color;
            Gizmos.DrawWireCube(cellWorldPosition, _tilemap.cellSize);
        }
    }
}