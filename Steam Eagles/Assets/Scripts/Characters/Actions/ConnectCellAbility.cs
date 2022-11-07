using System;
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
        return !IsCellBlocked(cellPosition) 
               && !Tilemap.HasTile(cellPosition) 
               && (!limitAdjacentNeighbors 
                   ||(GetAdjacentNeighborCount(cellPosition, true, ref blockedByNeighbor) < maxAdjacentNeighbors  
                      && !blockedByNeighbor));
    }
    
    int GetAdjacentNeighborCount(Vector3Int cellPosition, bool checkNeighborsRecursively, ref bool blockedByNeighbor)
    {
        int neighborCount = 0;
        foreach (var vector3Int in neighbors)
        {
            var neighborPosition = cellPosition + vector3Int;
            if (checkNeighborsRecursively)
            {
                int neighborNeighborCount = GetAdjacentNeighborCount(cellPosition, false, ref blockedByNeighbor);
                if (neighborNeighborCount > (maxAdjacentNeighbors -1))
                {
                    blockedByNeighbor = true;
                    return int.MaxValue;
                }
                neighborCount += neighborNeighborCount;
            }
            if (Tilemap.HasTile(neighborPosition))
            {
                neighborCount++;
            }
            {
                neighborCount++;
            }
        }

        return neighborCount;
    }

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        Tilemap.SetTile(cell, tileToPlace);
        MessageBroker.Default.Publish(new BuildActionInfo(Tilemap, cell));
    }
}