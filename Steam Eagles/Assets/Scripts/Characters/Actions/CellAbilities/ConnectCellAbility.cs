using System;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
[System.Obsolete("Ability System is deprecated, use Tool System instead.")]
public class ConnectCellAbility : CellAbility
{
    [SerializeField] private TileBase tileToPlace;
    [SerializeField] private SharedInt inventoryCount;
    [Header("Limit Number of Neighbors")]
    [SerializeField] private bool limitAdjacentNeighbors = false;
    [SerializeField, Range(0, 4)] private int maxAdjacentNeighbors = 2;
    private Vector3Int[] neighbors = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    

    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        if (!tilemap.HasValue) return false;
        if (!HasBlockInInventory(abilityUser)) return false;
        if (IsBlockedByNeighbor(cellPosition)) return false;
        if (IsCellBlocked(cellPosition)) return false;
        if(Tilemap.HasTile(cellPosition)) return false;
        return true;
    }

    bool HasBlockInInventory(AbilityUser abilityUser)
    {
        if (inventoryCount != null)
        {
            return inventoryCount.Value > 0;
        }

        return true;
    }
    bool IsBlockedByNeighbor(Vector3Int cellPosition)
    {
        if (!limitAdjacentNeighbors) return false;
        int cnt = 0;
        int cellNeighbors = GetNeighbors(cellPosition);
        if (cellNeighbors > maxAdjacentNeighbors)
            return true;
        foreach (var i in neighbors.Where(HasTile).Select(GetNeighbors))
        {
            if (i >= maxAdjacentNeighbors)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTile(Vector3Int neighbor)
    {
        return Tilemap.GetTile(neighbor) == tileToPlace;
    }

    int GetNeighbors(Vector3Int cellPosition)
    {
        int cnt = 0;
        return neighbors.Select(t=> cellPosition+t).Select(t=> HasTile(t)  ? 1 : 0).Sum();
    }
   
    

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        Tilemap.SetTile(cell, tileToPlace);
        if (inventoryCount != null)
        {
            inventoryCount.Value--;
            Debug.Assert(inventoryCount.Value >= 0);
        }
        MessageBroker.Default.Publish(new BuildActionInfo(Tilemap, cell));
    }
}