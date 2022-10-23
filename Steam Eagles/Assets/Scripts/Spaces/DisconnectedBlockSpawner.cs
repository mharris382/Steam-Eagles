using CoreLib;
using UniRx;

public class DisconnectedBlockSpawner : DynamicBlockSpawner
{
    private void Awake()
    {
        MessageBroker.Default.Receive<DisconnectActionInfo>().TakeUntilDestroy(this).Subscribe(OnBlockDisconnected);
    }

    void OnBlockDisconnected(DisconnectActionInfo disconnectActionInfo)
    {
        var wp = disconnectActionInfo.tilemap.GetCellCenterWorld(disconnectActionInfo.cellPosition);
        SpawnBlock(wp);
    }
}