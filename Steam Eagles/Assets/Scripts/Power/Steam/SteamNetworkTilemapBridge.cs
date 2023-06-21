using System.Collections;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Cysharp.Threading.Tasks;
using Power.Steam.Network;
using UniRx;
using UnityEngine;
using Zenject;


/// <summary>
/// syncs the pipe tilemap with the steam network
/// </summary>
public class SteamNetworkTilemapBridge : IInitializable
{
    private readonly Building _building;
    private readonly INetwork _network;
    private readonly CoroutineCaller _coroutineCaller;

    public SteamNetworkTilemapBridge(Building building, INetwork network, CoroutineCaller coroutineCaller)
    {
        _building = building;
        _network = network;
        _coroutineCaller = coroutineCaller;
    }
    public void Initialize()
    {
        var map = _building.Map;
        _coroutineCaller.StartCoroutine((IEnumerator)UniTask.ToCoroutine(async () =>
        {
            Debug.Log("Waiting for building to load before initializing steam network", _building);
            await UniTask.WaitUntil(() => _building.IsFullyLoaded);
            Debug.Log("Building loaded, initializing steam network", _building);
            var pipeCells = _building.Map.GetAllNonEmptyCells(BuildingLayers.PIPE).Select(t => (Vector2Int)t).ToArray();
            Debug.Log($"Found {pipeCells.Length} pipe cells", _building);
            InitializeNetwork(pipeCells);
            var rooms = _building.Map.GetAllBoundsForLayer(BuildingLayers.PIPE);
            Debug.Log("Starting state load");
            await UniTask.WhenAll(rooms.Select(t => (t.Item1, t.Item2.GetComponent<RoomTextures>())).Select(t => LoadRoomFromTexture(t.Item1, t.Item2)));
            Debug.Log("Finished state load");
            map.OnTileCleared2D(BuildingLayers.PIPE).Where(_network.HasPosition).Subscribe(_network.RemoveNode).AddTo(_building);
            map.OnTileSet2D(BuildingLayers.PIPE).Where(t => !_network.HasPosition(t.cell)).Subscribe(t => _network.AddNode(t.cell, NodeType.PIPE)).AddTo(_building);
        }));
            
      
    }

    private async UniTask LoadRoomFromTexture(BoundsInt boundsInt, RoomTextures roomTextures)
    {
        Debug.Log($"Waiting For Room Textures {roomTextures.name} PIPE Texture assignment", _building);
        await UniTask.WaitUntil(() => roomTextures.PipeTexture != null);
        Debug.Log($"Room Textures {roomTextures.name} PIPE Texture assignment complete", _building);
        _network.LoadSteamStateForTexture(boundsInt, roomTextures.PipeTexture);
    }

    private void InitializeNetwork(Vector2Int[] pipeCells)
    {
        for (int i = 0; i < pipeCells.Length; i++)
        {
            _network.AddNode(pipeCells[i], NodeType.PIPE);
        }
    }

        

}