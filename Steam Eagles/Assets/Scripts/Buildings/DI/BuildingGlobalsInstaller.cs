using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Messages;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;
using CoreLib;
using CoreLib.Entities;
using SaveLoad;
using Sirenix.Utilities;
using UniRx;
using Utilities.AddressablesUtils;


public class BuildingGlobalsInstaller : MonoInstaller
{
    public GlobalBuildingConfig config;
    [HideLabel] public TileAssets tileAssets;
    public ElectricityConfig electricityConfig;
    public override void InstallBindings()
    {
        Container.Bind<GlobalBuildingConfig>().FromInstance(config).AsSingle().NonLazy();
        //Container.Bind<MachineCellMap>().AsSingle().NonLazy();
        Container.Bind<BuildingRegistry>().AsSingle().NonLazy();
        Container.Bind<TileAssets>().FromInstance(tileAssets).AsSingle().NonLazy();
        ReflectedInstaller<ILayerSpecificRoomTexSaveLoader>.Install(Container, ContainerLevel.PROJECT);
        Container.BindInterfacesTo<EntityRoomTrackerV2>().AsSingle().NonLazy();
        
        Container.Bind<ElectricityConfig>().FromInstance(electricityConfig).AsSingle().NonLazy();
    }
}


public class EntityRoomTrackerV2 : IInitializable, ITickable, IDisposable
{
    private static EntityType[] dynamicTypes = new EntityType[]
    {
        EntityType.NPC, EntityType.ENEMY, EntityType.PLAYER, EntityType.VEHICLE, EntityType.CHARACTER
    };
    private readonly EntityLinkRegistry _entityLinkRegistry;
    private readonly BuildingRegistry _buildingRegistry;
    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    private readonly Queue<EntityInitializer> _removalQueue = new();
    private readonly Queue<EntityInitializer> _additionDynamicQueue = new();
    private readonly Dictionary<EntityInitializer, IDisposable> _dynamicEntities = new();
    HashSet<EntityType> trackedTypes = new();
    public EntityRoomTrackerV2(EntityLinkRegistry entityLinkRegistry, BuildingRegistry buildingRegistry)
    {
        _entityLinkRegistry = entityLinkRegistry;
        _buildingRegistry = buildingRegistry;
        trackedTypes.AddRange(dynamicTypes);
    }
    public void Initialize()
    {
        
        
        
        var addedTrackedStream = _entityLinkRegistry.OnValueAdded
            .StartWith(_entityLinkRegistry.Values)
            .Where(t => trackedTypes.Contains(t.GetEntityType()));
        
        var removeTrackedStream = _entityLinkRegistry.OnValueRemoved.Where(t => trackedTypes.Contains(t.GetEntityType()));

        addedTrackedStream.Subscribe(t => _additionDynamicQueue.Enqueue(t)).AddTo(_disposables);
        removeTrackedStream.Subscribe(t => _removalQueue.Enqueue(t)).AddTo(_disposables);
    }

    public void Tick()
    {
        foreach (var entityInitializer in _entityLinkRegistry.Values)
        {
            if (!_dynamicEntities.ContainsKey(entityInitializer) && trackedTypes.Contains(entityInitializer.GetEntityType()))
            {
                _additionDynamicQueue.Enqueue(entityInitializer);
            }
        }

        foreach (var entity in _dynamicEntities.Keys)
        {
            if (entity == null) continue;
            if (!_entityLinkRegistry.Contains(entity))
            {
                _removalQueue.Enqueue(entity);
            }
        }
        RemoveEntities();
        AddEntities();
        foreach (var dynamicEntity in _dynamicEntities.Keys)
        {
            if (dynamicEntity == null) continue;
            TrackEntity(dynamicEntity);
        }
    }

    private void TrackEntity(EntityInitializer dynamicEntity)
    {
        var trackingInfo = dynamicEntity.TrackingInfo;
        var lastRoom = trackingInfo.LastSeenRoom as Room;
        var lastBuilding= lastRoom != null ? lastRoom.Building : null;
        Room currentRoom = null;
        if (lastRoom != null && lastBuilding != null)
        {
            if (IsEntityInsideBuilding(dynamicEntity, lastBuilding, lastRoom, out currentRoom))
            {
                UpdateEntityInfo(currentRoom.Building, currentRoom);
                return;
            }
        }
        foreach (var building in _buildingRegistry.Buildings)
        {
            if (CanEntityBeInside(dynamicEntity, building))
            {
                if (IsEntityInsideBuilding(dynamicEntity, building, lastRoom, out currentRoom))
                {
                    UpdateEntityInfo(currentRoom.Building, currentRoom);
                    return;
                }
            }
        }
        UpdateEntityInfo(null, null);

        void UpdateEntityInfo(Building b, Room r)
        {
            trackingInfo.LastSeenBuilding = b;
            trackingInfo.LastSeenRoom = r;
            var entityChangedRoom = new EntityChangedRoomMessage(dynamicEntity, r);
            MessageBroker.Default.Publish(entityChangedRoom);
            if(!dynamicEntity.gameObject.TryGetComponent<EntityRoomState>(out var eState)) eState = dynamicEntity.gameObject.AddComponent<EntityRoomState>();
            if (!dynamicEntity.TryGetComponent<EntityRoomState>(out var lState)) lState = dynamicEntity.gameObject.AddComponent<EntityRoomState>();
            eState.SetCurrentRoom(r);
            lState.SetCurrentRoom(r);
        }
    }
    
    

    #region [Helpers]

    bool IsEntityInside(EntityInitializer entityInitializer, Room room)
    {
        var pos = entityInitializer.transform.position;
        if (room == null) return false;
        return room.ContainsWorldPosition(pos);
    }
   
    bool IsEntityInsideNeighborRoom(EntityInitializer entityInitializer, Room room, out Room newRoom)
    {
        var pos = entityInitializer.transform.position;
        newRoom = null;
        if (room == null)
            return false;
        foreach (var roomConnectedRoom in room.connectedRooms)
        {
            if (roomConnectedRoom.ContainsWorldPosition(pos))
            {
                newRoom = roomConnectedRoom;
                return true;
            }
        }
        return false;
    }

    bool IsEntityInsideBuilding(EntityInitializer entity, Building building, Room lastRoom, out Room newRoom)
    {
        newRoom = null;
        if (IsEntityInside(entity, lastRoom))
        {
            newRoom = lastRoom;
            return true;
        }
        if (IsEntityInsideNeighborRoom(entity, lastRoom, out newRoom))
        {
            return true;
        }

        if (building == null) return false;
        foreach (var room in building.Rooms.AllRooms)
        {
            if (IsEntityInside(entity, room))
            {
                newRoom = room;
                return true;
            }
        }
        return false;
    }
    bool CanEntityBeInside(EntityInitializer entityInitializer, Building building)
    {
        var pos = entityInitializer.transform.position;
        var sizeWs = building.sizeWorldSpace;
        var inside = sizeWs.Contains(pos);
        return inside;
        return building.WorldSpaceBounds.Contains(pos);
    }
    private void RemoveEntities()
    {
        while (_removalQueue.Count > 0)
        {
            var toRemove = _removalQueue.Dequeue();
            if(toRemove != null && _dynamicEntities.ContainsKey(toRemove))
            {
                _dynamicEntities[toRemove]?.Dispose();
                _dynamicEntities.Remove(toRemove);
            }
        }
    }

    private void AddEntities()
    {
        while (_additionDynamicQueue.Count > 0)
        {
            var toAdd = _additionDynamicQueue.Dequeue();
            if (toAdd != null && _dynamicEntities.ContainsKey(toAdd) == false)
            {
                var disposable = toAdd.TrackingInfo.LastSeenRoomProperty
                    .Select(t => new EntityChangedRoomMessage(toAdd, t as Room))
                    .Subscribe(MessageBroker.Default.Publish);
                _dynamicEntities.Add(toAdd, disposable);
            }
        }
    }

    #endregion
    public void Dispose()
    {
        _disposables.Dispose();
    }
}

[InlineProperty, HideLabel]
public class GlobalBuildingConfig : ConfigBase { }



