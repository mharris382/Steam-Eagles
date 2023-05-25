using CoreLib.Entities;
using UnityEngine;

namespace Weather.Storms
{
    public abstract class EntityStormSubject : IStormSubject
    {
        private readonly EntityInitializer _entityHook;

        public EntityStormSubject(GameObject obj)
        {
            _entityHook = obj.GetComponent<EntityInitializer>();
            Debug.Assert(_entityHook != null, "Entity GameObject missing EntityInitializer",obj);
        }

        public EntityType SubjectEntityType =>
            _entityHook != null ? _entityHook.GetEntityType() : EntityType.UNSPECIFIED;
    }
}