then create an interface to reverse the operation

```cs
public interface IRepairableTilemap : IBuildingTilemap
{
	IEnumerable<(RepairableTile tile, Vector2Int cellPosition)> GetAllRepairableTiles();
}
```
