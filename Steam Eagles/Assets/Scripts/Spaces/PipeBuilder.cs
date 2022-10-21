using System;
using System.Collections;
using StateMachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using World;

namespace Spaces
{
    public class PipeBuilder : MonoBehaviour
    {
        [Tooltip("Tile to build on map")]
        public TileBase pipeTile;
        
        [Tooltip("map to build onto")]
        public SharedTilemap targetTilemap;
        
        [Tooltip("Cannot build tile if the cell is blocked by the blocking tilemap")]
        public SharedTilemap blockingTilemap;


        [SerializeField] private Transform previewObject;
        
        
        private ICellSelector selector;
        

        
    }

    

    public struct SelectedCellInfo
    {
        public SelectedCellInfo(Tilemap tm, Vector3Int cellPosition, bool isBlocked, bool isInRange)
        {
            IsCellBlocked = isBlocked;
            IsCellInRange = isInRange;
            CellPosition = cellPosition;
            Tilemap = tm;
        }
        
        public Tilemap Tilemap { get; }
        public Vector3Int CellPosition { get; }

        public bool IsCellBlocked { get; }

        public bool IsCellInRange { get; set; }
    }

    [System.Obsolete("Use Characters.ICellSelector")]
    public interface ICellSelector
    {
        Vector3Int GetSelectedCell();

        void SetTargetTilemap(Tilemap tilemap);
    }
}