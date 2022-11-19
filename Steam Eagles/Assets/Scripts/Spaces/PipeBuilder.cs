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


    [System.Obsolete("Use Characters.ICellSelector")]
    public interface ICellSelector
    {
        Vector3Int GetSelectedCell();

        void SetTargetTilemap(Tilemap tilemap);
    }
}