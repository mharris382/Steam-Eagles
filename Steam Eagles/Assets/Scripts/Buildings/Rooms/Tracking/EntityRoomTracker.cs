using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Messages;
using CoreLib;
using CoreLib.Entities;
using UniRx;
using UnityEngine;

namespace Buildings.Rooms.Tracking
{
    [RequireComponent(typeof(Building))]
    public class EntityRoomTracker : MonoBehaviour
    {
        private ReactiveCollection<Entity> _trackedEntities = new ReactiveCollection<Entity>();
        private Dictionary<Entity, Room> _entityRoomMap = new Dictionary<Entity, Room>();
        private List<Entity> _entitiesWithoutRoom = new List<Entity>();
        private Building _building;
        
        public Building Building => _building ??= GetComponent<Building>();
        
        private void Awake()
        {
            _building = GetComponent<Building>();
            
            MessageBroker.Default.Receive<Entity>()
                .Where(t => t != null && !_trackedEntities.Contains(t) && t.entityType != EntityType.STRUCTURE || t.entityType != EntityType.BUILDING)
                .Subscribe(_trackedEntities.Add)
                .AddTo(this);

            void TrackEntity(Entity e)
            {
                if (_trackedEntities.Contains(e) == false)
                {
                    Debug.Log($"Now Tracking Entity {e.name.Bolded()} in building {Building.name.Bolded()}");
                    _trackedEntities.Add(e);
                }
            }
            MessageBroker.Default.Receive<EntityInitializedInfo>()
                .Where(t => t.entity.entityType == EntityType.CHARACTER || t.entity.entityType == EntityType.ENEMY ||
                            t.entity.entityType == EntityType.NPC)
                .Subscribe(t => TrackEntity(t.entity)).AddTo(this);

            foreach (var entity in EntityManager.Instance.GetAllEntities())
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

        private void Update()
        {
            foreach (var trackedEntity in _trackedEntities)
            {
                if (!_entityRoomMap.ContainsKey(trackedEntity))
                {
                    var room = SearchForEntityInBuilding(trackedEntity);
                    if (room == null)
                    {
                        Debug.LogWarning($"Couldn't Find Entity {trackedEntity.name.Bolded()} in building {Building.name.Bolded()}");
                        continue;
                    }
                    UpdateEntityRoom(trackedEntity, room);
                }

                if (trackedEntity.LinkedGameObject == null)
                {
                    Debug.LogWarning("Cannot track entity without a linked game object", trackedEntity);
                    continue;
                }

                if (!trackedEntity.dynamic)
                    continue;
                
                CheckForDynamicEntityChangedRooms(trackedEntity);
            }
        }

        private void CheckForDynamicEntityChangedRooms(Entity trackedEntity)
        {
            if (!_entityRoomMap.ContainsKey(trackedEntity))
            {
                
                return;
            }
            var lastSeenRoom = _entityRoomMap[trackedEntity];
            var currentPositionWs = trackedEntity.LinkedGameObject.transform.position;
            var currentPositionRs = Building.transform.InverseTransformPoint(currentPositionWs);
            //Entity is still in the same room
            if (lastSeenRoom.RoomBounds.Contains(currentPositionRs))
                return;
            
            //Entity has most likely moved to a new room that is adjacent to the last seen room
            if (Building.Map.RoomGraph.Graph.TryGetOutEdges(lastSeenRoom, out var neighbors))
            {
                foreach (var room in neighbors.Select(t => t.Target))
                {
                    if (room.RoomBounds.Contains(currentPositionRs))
                    {
                        UpdateEntityRoom(trackedEntity, room);
                    }
                }
            }
            //Entity has moved to a room that is not adjacent to the last seen room, need to perform a full search to re-locate the entity
            else
            {
                var room = SearchForEntityInBuilding(trackedEntity);
                if (room == null)
                {
                    throw new NotImplementedException(
                        $"Entity {trackedEntity.name} that was previously inside a room has moved to " +
                        $"a room that is not adjacent to the last seen room, and could not be found in the building");
                }
                UpdateEntityRoom(trackedEntity, room);
            }
        }

        private void UpdateEntityRoom(Entity trackedEntity, Room room)
        {
           
            
            if (_entityRoomMap.ContainsKey(trackedEntity))
            {
                var previous = _entityRoomMap[trackedEntity];
                Debug.Log($"Entity {trackedEntity.name.Bolded()} has moved from {previous.name.InItalics()} to {room.name.Bolded().InItalics()}");
                _entityRoomMap[trackedEntity] = room;
            }
            else
            {
                Debug.Log($"Found Entity {trackedEntity.name.Bolded()} in room {room.name.Bolded()}");
                _entityRoomMap.Add(trackedEntity, room);
            }
            MessageBroker.Default.Publish(new EntityChangedRoomMessage(trackedEntity, room));
            if(!trackedEntity.gameObject.TryGetComponent<EntityRoomState>(out var eState)) eState = trackedEntity.gameObject.AddComponent<EntityRoomState>();
            if (!trackedEntity.LinkedGameObject.TryGetComponent<EntityRoomState>(out var lState)) lState = trackedEntity.LinkedGameObject.AddComponent<EntityRoomState>();
            eState.SetCurrentRoom(room);
            lState.SetCurrentRoom(room);
        }

        private Room SearchForEntityInBuilding(Entity entity)
        {
            if (entity.LinkedGameObject == null)
            {
               Debug.LogWarning($"Cannot search for entity without a linked game object, {entity.name}");
               return null;
            }

            var lsPos = Building.transform.InverseTransformPoint(entity.LinkedGameObject.transform.position);
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