using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

[System.Obsolete("Ability system prototypes will be phased out and replaced by the tool system")]
public class DisconnectCellAbility : CellAbility
{

    public UnityEvent<Vector3> onDisconnectedBlock;
    public override bool CanPerformAbilityOnCell(AbilityUser abilityUser, Vector3Int cellPosition)
    {
        if (!tilemap.HasValue) return false;
        //var wp = Tilemap.GetCellCenterWorld(cellPosition);
        //var tp = transform.position;
        if(IsCellBlocked(cellPosition)) return false;
        if (!Tilemap.HasTile(cellPosition)) return false;
        return true;
        throw new System.NotImplementedException();
    }

    public override void PerformAbilityOnCell(AbilityUser abilityUser,Vector3Int cell)
    {
        MessageBroker.Default.Publish(new DisconnectActionInfo(Tilemap, cell));
        Tilemap.SetTile(cell, null);
        onDisconnectedBlock?.Invoke(Tilemap.GetCellCenterWorld(cell));
    }
}