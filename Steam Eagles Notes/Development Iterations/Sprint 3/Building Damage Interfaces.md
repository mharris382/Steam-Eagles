
```cs
public interface IDamageableTilemap : IBuildingTilemap
{
	IEnumerable<(DamageableTile tile, Vector2Int cellPosition)> GetAllDamageableTiles();

	
}
```

![[Building Repair Interfaces]]

%% seealso [[Building Interfaces]] %%