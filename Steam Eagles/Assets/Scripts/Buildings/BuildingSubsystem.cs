using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Buildings
{
    public abstract class BuildingSubsystem : MonoBehaviour
    {
        private Building _building;

        public Building Building => _building != null ? _building : _building = GetComponent<Building>();

        public Transform entityParent;
        public bool HasEntityParent => entityParent != null;
    }
    
    
    
    public abstract class BuildingSubsystem<T> : BuildingSubsystem where T : Component, IEntityID
    {
        private ReactiveCollection<T> _entity = new ReactiveCollection<T>();

        public ReactiveCollection<T> RegisteredEntities => _entity;

        private Dictionary<string, T> _registeredEntities = new Dictionary<string, T>();

        public abstract void OnSubsystemEntityRegistered(T entity);
        public abstract void OnSubsystemEntityUnregistered(T entity);

        public void RegisterSubsystemEntity(T e)
        {
            if (_registeredEntities.ContainsKey(e.GetEntityGUID()))
            {
                Debug.LogWarning($"Already registered entity {e.GetEntityGUID()}", e);
                return;
            }
            Debug.Log($"Registering {e.name} on {name}");
            _entity.Add(e);
            OnSubsystemEntityRegistered(e);
        }

        public void UnregisterSubsystemEntity(T buildingMechanism)
        {
            if (!_registeredEntities.ContainsKey(buildingMechanism.GetEntityGUID()))
            {
                return;
            }
            Debug.Log($"Unregistering {buildingMechanism.name} on {name}");
            _entity.Remove(buildingMechanism);
            OnSubsystemEntityUnregistered(buildingMechanism);
        }
        
        
        public IEnumerable<T> FindAllEntities()
        {
            var entities = GetComponentsInChildren<T>();
            foreach (var entity in entities)
            {
                RegisterSubsystemEntity(entity);
            }
            return entities;
        }
    }
}