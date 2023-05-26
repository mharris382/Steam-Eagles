using System;
using CoreLib.Entities;
using UniRx;
using UnityEngine;

namespace Weather.Storms
{
    public abstract class EntityStormSubject : IStormSubject, IDisposable
    {
        private readonly EntityInitializer _entityHook;
        protected readonly CompositeDisposable _cd;

        public abstract Bounds SubjectBounds { get; }

        public EntityStormSubject(GameObject obj)
        {
            _entityHook = obj.GetComponent<EntityInitializer>();
            Debug.Assert(_entityHook != null, "Entity GameObject missing EntityInitializer",obj);
            _cd = new CompositeDisposable();
        }

        public EntityType SubjectEntityType =>
            _entityHook != null ? _entityHook.GetEntityType() : EntityType.UNSPECIFIED;


        public abstract void OnStormAdded(Storm storm);
        public abstract void OnStormRemoved(Storm storm);

        public void Dispose()
        {
            _cd?.Dispose();
        }
    }
}