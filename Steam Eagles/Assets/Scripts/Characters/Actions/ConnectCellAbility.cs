using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ConnectCellAbility : CellAbility
{
    [SerializeField]
    private TileBase tileToPlace;
    
    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        return !IsCellBlocked(cellPosition) && !Tilemap.HasTile(cellPosition);
    }

    public override void PerformAbilityOnCell(Vector3Int cell)
    {
        Tilemap.SetTile(cell, tileToPlace);
        MessageBroker.Default.Publish(new BuildActionInfo(Tilemap, cell));
    }
}