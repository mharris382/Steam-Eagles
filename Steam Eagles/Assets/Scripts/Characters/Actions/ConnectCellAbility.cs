using System;
using System.Linq;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConnectCellAbility : CellAbility
{
    [SerializeField] private TileBase tileToPlace;
    
    [Header("Limit Number of Neighbors")]
    [SerializeField] private bool limitAdjacentNeighbors = false;
    [SerializeField, Range(0, 4)] private int maxAdjacentNeighbors = 2;
    private Vector3Int[] neighbors = new Vector3Int[] { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

    

    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        bool blockedByNeighbor = false;
        if (limitAdjacentNeighbors)
        {
            int cnt = 0;
            foreach (var neighbor in neighbors.Select(t=> cellPosition+t))
            {
                if (Tilemap.HasTile(neighbor))
                {
                    cnt++;
                    if(cnt >= maxAdjacentNeighbors)
                    {
                        blockedByNeighbor = true;
                        break;
                    }
                    else if (GetNeighbors(neighbor) >= maxAdjacentNeighbors)
                    {
                        blockedByNeighbor = true;
                        break;
                    }
                }
            }
        }
        return !blockedByNeighbor && !IsCellBlocked(cellPosition)
               && !Tilemap.HasTile(cellPosition);
    }

    int GetNeighbors(Vector3Int cellPosition)
    {
        int cnt = 0;
        return neighbors.Select(t=> cellPosition+t).Select(t=> Tilemap.HasTile(t)  ? 1 : 0).Sum();
    }
   
    

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        Tilemap.SetTile(cell, tileToPlace);
        MessageBroker.Default.Publish(new BuildActionInfo(Tilemap, cell));
    }
}