using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Messages;
using CoreLib;
using CoreLib.MyEntities;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    [RequireComponent(typeof(Building))]
    public class EntityRoomTracker : MonoBehaviour
    {
        private ReactiveCollection<EntityInitializer> _trackedEntities = new ReactiveCollection<EntityInitializer>();
        private Dictionary<EntityInitializer, Room> _entityRoomMap = new Dictionary<EntityInitializer, Room>();
        private List<EntityInitializer> _entitiesWithoutRoom = new List<EntityInitializer>();
        private Building _building;
        private EntityLinkRegistry linkRegistry;

        public Building Building => _building ??= GetComponent<Building>();

        [Inject]
        void InjectMe(EntityLinkRegistry linkRegistry)
        {
            this.linkRegistry = linkRegistry;
            linkRegistry.OnValueAdded.Where(t => t != null && t.GetEntityType() != EntityType.STRUCTURE || t.GetEntityType() != EntityType.BUILDING)
                .Subscribe(t => _trackedEntities.Add(t))
                .AddTo(this);
        }
        
        private void Start()
        {
            _building = GetComponent<Building>();
            
            // MessageBroker.Default.Receive<Entity>()
            //     .Where(t => t != null && !_trackedEntities.Contains(t) && t.entityType != EntityType.STRUCTURE || t.entityType != EntityType.BUILDING)
            //     .Subscribe(_trackedEntities.Add)
            //     .AddTo(this);
            //
             void TrackEntity(EntityInitializer e)
             {
                 if (_trackedEntities.Contains(e) == false)
                 {
                     Debug.Log($"Now Tracking Entity {e.name.Bolded()} in building {Building.name.Bolded()}");
                     _trackedEntities.Add(e);
                 }
             }
            // MessageBroker.Default.Receive<EntityInitializedInfo>()
            //     .Where(t => t.entity.entityType == EntityType.CHARACTER || t.entity.entityType == EntityType.ENEMY ||
            //                 t.entity.entityType == EntityType.NPC)
            //     .Subscribe(t => TrackEntity(t.entity)).AddTo(this);

            foreach (var entity in linkRegistry.Values.Where(t => t != null && t.GetEntityType() != EntityType.STRUCTURE || t.GetEntityType() != EntityType.BUILDING))
            {
                var room = SearchForEntityInBuilding(entity);
                if (room == null)
                {
                    _entitiesWithoutRoom.Add(entity);
                }
                else
                {
                    _trackedEntities.Add(entity);
                    UpdateEntityRoom(entity, room);
                }
            }


            _trackedEntities.ObserveAdd()
                .Select(t => (t.Value, SearchForEntityInBuilding(t.Value)))
                .Where(t => t.Item2 != null)
                .Subscribe(t => UpdateEntityRoom(t.Item1, t.Item2))
                .AddTo(this);
        }

      

        private void CheckForDynamicEntityChangedRooms(EntityInitializer trackedEntity)
        {
            if (!_entityRoomMap.ContainsKey(trackedEntity))
            {
                
                return;
            }
            var lastSeenRoom = _entityRoomMap[trackedEntity];
            var currentPositionWs = trackedEntity.transform.position;
            var currentPositionRs = Building.transform.InverseTransformPoint(currentPositionWs);
            
            //Entity is still in the same room
            if (lastSeenRoom!= null && lastSeenRoom.RoomBounds.Contains(currentPositionRs))
                return;
            
            //Entity has most likely moved to a new room that is adjacent to the last seen room
            if (lastSeenRoom!= null && Building.Map.RoomGraph.Graph.TryGetOutEdges(lastSeenRoom, out var neighbors))
            {
                foreach (var r in neighbors.Select(t => t.Target))
                {
                    if (r.RoomBounds.Contains(currentPositionRs))
                    {
                        UpdateEntityRoom(trackedEntity, r);
                        return;
                    }
                }
            }
            //Entity has moved to a room that is not adjacent to the last seen room, need to perform a full search to re-locate the entity
            
            var room = SearchForEntityInBuilding(trackedEntity);
            if(room != lastSeenRoom) UpdateEntityRoom(trackedEntity, room);
            
    }

        private void UpdateEntityRoom(EntityInitializer trackedEntity, Room room)
        {
           
            
            if (_entityRoomMap.ContainsKey(trackedEntity))
            {
                var previous = _entityRoomMap[trackedEntity];
                if(room != null && previous != null)
                    Debug.Log($"Entity {trackedEntity.name.Bolded()} has moved from {previous.name.InItalics()} to {room.name.Bolded().InItalics()}");
                else if(room == null && previous != null) Debug.Log($"Entity {trackedEntity.name.Bolded()} has moved from {previous.name.InItalics()} to outside the building");
                else if(room != null && previous == null) Debug.Log($"Entity {trackedEntity.name.Bolded()} has moved from outside the building to {room.name.InItalics()}");
                _entityRoomMap[trackedEntity] = room;
            }
            else
            {
                if(room != null)
                    Debug.Log($"Found Entity {trackedEntity.name.Bolded()} in room {room.name.Bolded()}");
                else Debug.Log($"Found Entity {trackedEntity.name.Bolded()} outside the building");
                _entityRoomMap.Add(trackedEntity, room);
            }
            MessageBroker.Default.Publish(new EntityChangedRoomMessage(trackedEntity, room));
            if(!trackedEntity.gameObject.TryGetComponent<EntityRoomState>(out var eState)) eState = trackedEntity.gameObject.AddComponent<EntityRoomState>();
            if (!trackedEntity.TryGetComponent<EntityRoomState>(out var lState)) lState = trackedEntity.gameObject.AddComponent<EntityRoomState>();
            eState.SetCurrentRoom(room);
            lState.SetCurrentRoom(room);
        }

        private Room SearchForEntityInBuilding(EntityInitializer entity)
        {
            if (entity == null)
            {
               Debug.LogWarning($"Cannot search for entity without a linked game object, {entity.name}");
               return null;
            }

            var lsPos = Building.transform.InverseTransformPoint(entity.transform.position);
            foreach (var graphVertex in Building.Map.RoomGraph.Graph.Vertices)
            {
                if (graphVertex.RoomBounds.Contains(lsPos))
                {
                    return graphVertex;
                }
            }
            return null;
        }
    }
}