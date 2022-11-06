using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

public abstract class CellAbility : MonoBehaviour
{
    [SerializeField] private SharedTilemap tilemap;
    [SerializeField] public SharedTilemap blockingMap;
    
    [SerializeField] public List<SharedTilemap> blockingMaps;
    [SerializeField] public List<Tilemap> sceneBlockingMaps;
    public virtual Tilemap Tilemap => tilemap.Value;

    protected virtual bool IsCellBlocked(Vector3Int cell)
    {
        foreach (var sharedTilemap in blockingMaps.Where(t=> t.HasValue).Select(t => t.Value).Concat(sceneBlockingMaps.Where(t=>t!=null)))
        {
            if (sharedTilemap.HasTile(cell))
            {
                return true;
            }
        }
        if (blockingMap == null)
            return false;
        return blockingMap.Value.HasTile(cell);
    }
    public abstract bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition);

    public abstract void PerformAbilityOnCell(AbilityUser user, Vector3Int cell);

    private void Awake()
    {
        
    }

    public virtual int SortCellLocationsByPreference(Vector3Int cell1, Vector3Int cell2)
    {
        var diff = cell2 - cell1;
        if (diff.sqrMagnitude == 0) return 0;
        return diff.sqrMagnitude > 0 ? -1 : 1;
        bool cValid1 = CanPerformAbilityOnCell(null, cell1) && !IsCellBlocked(cell1);
        bool cValid2 = CanPerformAbilityOnCell(null, cell2) && !IsCellBlocked(cell2);
        if (!cValid1) return -1;
        if (!cValid2) return 1;
        return 0;
    }
}