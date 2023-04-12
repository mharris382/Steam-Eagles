using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
	[Obsolete("Use BuildingMap instead")]
    public interface IReadOnlyBuildingTilemap
    {
			public  string  StructureName{ get; }
			
			public BuildingLayers Layer { get; }
			
        	public Vector2 CellSize { get; }
        	
            public BoundsInt CellBounds { get; }
        	
        	public TileBase GetTile(Vector2Int position);
        	public T GetTile<T>(Vector2Int position) where T : TileBase;

            public IEnumerable<(TileBase t, Vector3Int c)> GetAllNonEmptyTiles();
            //public Dictionary<TileBase, List<Vector2Int>> TilePositionDictionary { get; }	
    }
    
    
    public interface IBuildingTilemap : IReadOnlyBuildingTilemap
    {
		public void SetTile(Vector2Int position, TileBase tile);
    }
}