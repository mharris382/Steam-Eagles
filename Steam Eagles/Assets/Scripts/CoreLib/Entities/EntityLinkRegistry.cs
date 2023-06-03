using System.Collections.Generic;
using UnityEngine;

namespace CoreLib.Entities
{
    public class EntityLinkRegistry : Registry<EntityInitializer>
    {
        private readonly EntityV2.Factory _entityFactory;
        private readonly Dictionary<string, EntityV2> _entityLookup = new();
        private readonly Dictionary<EntityType, HashSet<string>> _entityTypeLookup = new();


        public EntityLinkRegistry(EntityV2.Factory entityFactory)
        {
            _entityFactory = entityFactory;
        }

        protected override bool CanRegister(EntityInitializer value)
        {
            var result = _entityLookup.ContainsKey(value.GetEntityGUID());
            if (result)
            {
                Debug.LogError(
                    $"Cannot register entity with GUID {value.GetEntityGUID()} because it is already registered to", _entityLookup[value.GetEntityGUID()].LinkedGo);
            }
            return !result;
        }

        protected override void AddValue(EntityInitializer value)
        {
            var guid = value.GetEntityGUID();
            var type = value.GetEntityType();
            var linkedGo = value.gameObject;
            _entityLookup.Add(guid,_entityFactory.Create(value));
            if (!_entityTypeLookup.TryGetValue(type, out var set))
                _entityTypeLookup.Add(type, set = new HashSet<string>());
            _entityTypeLookup[type].Add(guid);
            base.AddValue(value);
        }

        protected override void RemoveValue(EntityInitializer value)
        {
            var guid = value.GetEntityGUID();
            var type = value.GetEntityType();
            var linkedGo = value.gameObject;
            
            if (_entityTypeLookup.TryGetValue(type, out var set))
            {
                set.Remove(guid);
            }
            
            var removedEntity = _entityLookup[guid];
            removedEntity.OnRemoved();
            _entityLookup.Remove(guid);
            
            base.RemoveValue(value);
        }

        public EntityV2 GetEntity(EntityInitializer initializer)
        {
            var guid = initializer.GetEntityGUID();
            if (!_entityLookup.ContainsKey(guid))
            {
                Debug.LogError($"Cannot find entity with GUID {guid}", initializer.gameObject);
                return null;
            }
            return _entityLookup[guid];
        }
        public IEnumerable<EntityV2> GetAllEntities() => _entityLookup.Values;

        public IEnumerable<EntityV2> GetAllEntitiesOfType(EntityType type)
        {
            if (!_entityTypeLookup.ContainsKey(type))
                yield break;
            var guids = _entityTypeLookup[type];
            foreach (var guid in guids)
            {
                yield return _entityLookup[guid];
            }
        }
    }
}