using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class DisconnectCellAbility : CellAbility
{

    public UnityEvent<Vector3> onDisconnectedBlock;
    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        //var wp = Tilemap.GetCellCenterWorld(cellPosition);
        //var tp = transform.position;
        return !IsCellBlocked(cellPosition) && Tilemap.HasTile(cellPosition);
        throw new System.NotImplementedException();
    }

    public override void PerformAbilityOnCell(Vector3Int cell)
    {
        Tilemap.SetTile(cell, null);
        onDisconnectedBlock?.Invoke(Tilemap.GetCellCenterWorld(cell));
        MessageBroker.Default.Publish(new DisconnectActionInfo(Tilemap, cell));
    }
}