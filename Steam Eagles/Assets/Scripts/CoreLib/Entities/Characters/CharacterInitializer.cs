using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities.Characters
{
    [RequireComponent(typeof(Character))]
    public class CharacterInitializer : EntityInitializer
    {
        private Character _character;
        
        [ShowInInspector]
        private IReadOnlyReactiveProperty<Entity> _listener;

        private Character Character => _character != null
            ? _character
            : _character = GetComponent<Character>();
        
        public override string GetEntityGUID() => tag;

        public override EntityType GetEntityType() => EntityType.CHARACTER;
        

        private void Awake()
        {
            Character.IsEntityInitialized = false;
            _listener = EntityManager.Instance.GetEntityProperty(GetEntityGUID());
        }

        public override void OnEntityInitialized(Entity entity)
        {
            Character.IsEntityInitialized = true;
            
            if (EntityManager.Instance.debug) 
                Debug.Log($"Initialized Entity for {entity.name}");
            
            //TODO: inject loaded entity into all character entity listener systems
        }
    }
}