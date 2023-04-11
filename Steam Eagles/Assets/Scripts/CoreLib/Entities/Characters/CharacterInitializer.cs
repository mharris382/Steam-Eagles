﻿using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace CoreLib.Entities.Characters
{
    [RequireComponent(typeof(CharacterState))]
    public class CharacterInitializer : EntityInitializer
    {
        private CharacterState _characterState;
        
        [ShowInInspector]
        private IReadOnlyReactiveProperty<Entity> _listener;

        private CharacterState CharacterState => _characterState != null
            ? _characterState
            : _characterState = GetComponent<CharacterState>();
        
        public override string GetEntityGUID() => tag;

        public override EntityType GetEntityType() => EntityType.CHARACTER;
        

        private void Awake()
        {
            CharacterState.IsEntityInitialized = false;
            _listener = EntityManager.Instance.GetEntityProperty(GetEntityGUID());
        }

        public override void OnEntityInitialized(Entity entity)
        {
            CharacterState.IsEntityInitialized = true;
            
            if (EntityManager.Instance.debug) 
                Debug.Log($"Initialized Entity for {entity.name}");
            
            //TODO: inject loaded entity into all character entity listener systems
        }
    }
}