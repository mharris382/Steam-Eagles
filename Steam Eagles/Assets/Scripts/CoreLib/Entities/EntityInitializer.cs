using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities
{
    public abstract class EntityInitializer : MonoBehaviour
    {
        public abstract string GetEntityGUID();
        public abstract EntityType GetEntityType();
        
        private ReactiveProperty<Entity> _entity = new ReactiveProperty<Entity>();
        
        public Entity Entity => _entity.Value;
        public IReadOnlyReactiveProperty<Entity> EntityProperty => _entity;

        /// <summary>
        /// once entity has been setup to store correct custom data (such as inventory data) and all
        /// save data has been loaded, this method is called to all implementing classes to then
        /// pass the data to the user once it is ready to be used
        /// </summary>
        /// <param name="entity"></param>
        public abstract void OnEntityInitialized(Entity entity);
        
        private IEnumerator Start()
        {
            while(EntityManager.Instance == null)
                yield return null;
            
            var entityGUID = GetEntityGUID();
            var entityType = GetEntityType();
            OnEntityInitialized(_entity.Value = EntityManager.Instance.GetEntity(this));
        }
        
        private void OnDestroy()
        {
            EntityManager.Instance.SaveEntity(GetEntityGUID());
            _entity.Dispose();
        }
    }
}