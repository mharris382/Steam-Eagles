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
                .Where(t => t != null && !_trackedEntities.Contains(t))
                .Subscribe(_trackedEntities.Add)
                .AddTo(this);

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
                }

                if (trackedEntity.linkedGameObject == null)
                {
                    Debug.LogWarning("Cannot track entity without a linked game object", trackedEntity);
                    continue;
                }
                var lastSeenRoom = _entityRoomMap[trackedEntity];
                var currentPositionWS = trackedEntity.linkedGameObject.transform.position;
                var currentPositionRS = Building.transform.InverseTransformPoint(currentPositionWS);
                if (lastSeenRoom.RoomBounds.Contains(currentPositionRS))
                {
                    //Entity is still in the same room
                }
                //Entity has most likely moved to a new room that is adjacent to the last seen room
                else if (Building.Map.RoomGraph.Graph.TryGetOutEdges(lastSeenRoom, out var neighbors))
                {
                    foreach (var room in neighbors.Select(t => t.Target))
                    {
                        if (room.RoomBounds.Contains(currentPositionRS))
                        {
                            UpdateEntityRoom(trackedEntity, room);
                        }
                    }
                }
                else
                {
                    //Entity has moved to a room that is not adjacent to the last seen room, need to perform a full search to re-locate the entity
                }
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
        }


        private Room SearchForEntityInBuilding(Entity entity)
        {
            if (entity.linkedGameObject == null)
            {
               Debug.LogWarning($"Cannot search for entity without a linked game object, {entity.name}");
               return null;
            }

            var lsPos = Building.transform.InverseTransformPoint(entity.linkedGameObject.transform.position);
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