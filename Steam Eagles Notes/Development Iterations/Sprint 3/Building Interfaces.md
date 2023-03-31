
```cs
public interface IReadOnlyBuildingTilemap
{
	public StructureName string { get; }
	public Vector2 CellSize { get; }
	public BoundsInt CellBounds { get; }
	
	public TileBase GetTile(Vector2Int position);
	public T GetTile<T>(Vector2Int position) where T : TileBase tile;
	
	public Dictionary<TileBase, List<Vector2Int> TilePositionDictioary { get; }	
}
```

```cs
public interface IBuildingTilemap
{
	
	public void SetTile(Vector2Int position, TileBase tile);
	}
```


that should be all the damage system needs although we could make an interface for damage specifically

![[Building Damage Interfaces]]