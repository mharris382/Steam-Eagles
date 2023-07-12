using System;
using Buildings;
using Cysharp.Threading.Tasks;
using Power;
using Power.Steam.Network;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using UniRx;

public class SteamGlobalInstaller : MonoInstaller
{
    public SteamNetworkTiles tiles;

    public override void InstallBindings()
    {
        Container.Bind<SteamNetworkTiles>().AsSingle().NonLazy();
    }
}

    
[Serializable]
public class SteamNetworkTiles
{
    public TileBase inflowTile;
    public TileBase outflowTile;
}



public class BuildingPipeNetwork : IInitializable, IDisposable
{
    private readonly Building _building;
    private readonly SteamNetworkTiles _tiles;
    private readonly SteamConsumers _consumers;
    private readonly SteamProducers _producers;
    private CompositeDisposable _cd = new();
    public BuildingPipeNetwork(
        Building building, 
        SteamNetworkTiles tiles, 
        SteamConsumers consumers, 
        SteamProducers producers)
    {
        _building = building;
        _tiles = tiles;
        _consumers = consumers;
        _producers = producers;
        foreach (var steamConsumer in _consumers)
        {
            SetTileAsConsumer(building, steamConsumer);
        }
        foreach (var steamProducer in _producers)
        {
            SetTileAsProducer(building, steamProducer);
        }
    }

    private void SetTileAsConsumer(Building building, (Vector2Int cell, ISteamConsumer value) steamConsumer)
    {
        building.Map.SetTile((Vector3Int)steamConsumer.cell, BuildingLayers.GAS, _tiles.inflowTile);
    }

    private void SetTileAsProducer(Building building, (Vector2Int cell, ISteamProducer value) steamProducer)
    {
        building.Map.SetTile((Vector3Int)steamProducer.cell, BuildingLayers.GAS, _tiles.outflowTile);
    }

    public void Initialize()
    {
        _consumers.OnSystemRemoved.Subscribe(t => _building.Map.SetTile((Vector3Int)t.Item1, BuildingLayers.GAS, null)).AddTo(_cd);
        _consumers.OnSystemAdded.Subscribe(t => SetTileAsConsumer(_building, t)).AddTo(_cd);
        _producers.OnSystemRemoved.Subscribe(t => _building.Map.SetTile((Vector3Int)t.Item1, BuildingLayers.GAS, null)).AddTo(_cd);
        _producers.OnSystemAdded.Subscribe(t => SetTileAsProducer(_building, t)).AddTo(_cd);
    }

    public void Dispose()
    {
        _cd.Dispose();
    }
}
