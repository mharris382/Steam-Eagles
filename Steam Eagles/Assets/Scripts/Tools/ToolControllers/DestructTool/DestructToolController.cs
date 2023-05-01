﻿using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Rooms;
using CoreLib;
using CoreLib.Interfaces;
using Items;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using ToolControllerBase = Tools.BuildTool.ToolControllerBase;

namespace Tools.DestructTool
{
    public partial class DestructToolController : ToolControllerBase
    {
        [Min(0)] [SerializeField, HideInInspector] private float minDistance = 0.5f;
        [Min(0)] [SerializeField, HideInInspector] private float maxDistance = 3f;

      //  [FoldoutGroup("Aiming Settings"),SerializeField] private Vector2 originOffset = new Vector2(0, 1);
      //  [FormerlySerializedAs("aimSpeed")] [FoldoutGroup("Aiming Settings"),SerializeField] private float maxAngleDelta = 3f;
      //  [FoldoutGroup("Aiming Settings"),SerializeField] private float aimSmoothing = 0.1f;
      //  [FoldoutGroup("Aiming Settings"), Range(-1, 1)] [SerializeField] private float aimSnapThreshold = 0.1f;
        
        [SerializeField] private DestructionConfig config;
        [SerializeField] private DestructionToolFeedbacks feedbacks;
        
        
        
        
        public Transform aimPoint;
        private Vector2 _actualAimPositionLs;
        private Vector2 _aimVelocity;
        private float _speed;
        private float _velAngle;
        private RaycastHit2D[] _cache = new RaycastHit2D[30];
        private Dictionary<Collider2D, IDestruct> _seenDestructables = new Dictionary<Collider2D, IDestruct>();
        private Dictionary<IDestruct, Destructor> _destructors = new Dictionary<IDestruct, Destructor>();
        private Vector2 _direction;


        #region [Properties]

        [FoldoutGroup("Aiming Settings"),PropertyOrder(-1),ShowInInspector, MinMaxSlider(0, 10)]
        public Vector2 ToolRange
        {
            get => new Vector2(minDistance, maxDistance);
            set
            {
                minDistance = Mathf.Min(value.x, value.y);
                maxDistance = Mathf.Max(value.x, value.y);
            }
        }

        public Vector2 AimDirection => AimHandler.AimDirection;

        public float CastDistance => maxDistance - minDistance;

        #endregion

        public override void OnToolEquipped()
        {
            Activator.IsEquipped = true;
            base.OnToolEquipped();
        }

        public override void OnToolUnEquipped()
        {
            Activator.IsEquipped = false;
            base.OnToolUnEquipped();
        }

        protected override void OnStart()
        {
            aimPoint.parent = transform;
            
        }

        public override BuildingLayers GetTargetLayer() => BuildingLayers.SOLID;

        protected override void OnRoomChanged(Room room)
        {
            HasRoom = room != null && room.buildLevel == BuildLevel.FULL;
        }

        public override ToolStates GetToolState()
        {
            return ToolStates.Destruct;
        }

        public override bool UsesRecipes(out List<Recipe> recipes)
        {
            recipes = null;
            return false;
        }

        private void Update()
        {
            if (!HasResources())
            {
                Debug.Log("No resources", this);
                return;
            }

            UpdateAim(Time.deltaTime);
            
            Activator.IsInUse = ToolState.Inputs.UseHeld;
            
            if(Activator.IsActive.Value)
                CheckForDestructables(Time.deltaTime);
        }


        private void UpdateAim(float dt)
        {
            AimHandler.UpdateAimPosition(BuildingLayers.SOLID);
        }


        private void CheckForDestructables(float dt)
        {
            using (new Physics2DQueryScope(hitTriggers: true, startInColliders: true))
            {
                var castStart = ToolState.aimOriginOffset + (Vector2)ToolState.transform.position;
                //int hits = Physics2D.OverlapCircleNonAlloc(position, config.radius, _cache, config.destructibleLayers);
                int hits = Physics2D.CircleCastNonAlloc(castStart, config.radius, AimDirection, _cache,
                    CastDistance, config.destructibleLayers);
                
                Debug.DrawRay(castStart, AimDirection.normalized * CastDistance, hits > 0 ? Color.red : Color.red.Lighten(0.5f), 0.1f);
               
                for (int i = 0; i < hits; i++)
                {
                    var destructable = GetDestructable(_cache[i].collider);
                    if (destructable == null)
                        continue;
                    NotifyHitDestructable(destructable, _cache[i], dt);
                }
            }
        }

        private IDestruct GetDestructable(Collider2D coll)
        {
            if (coll == null)
                return null;

            if (_seenDestructables.TryGetValue(coll, out var destructable))
                return destructable;

            destructable = coll.GetComponent<IDestruct>();
            if (destructable != null)
                _seenDestructables.Add(coll, destructable);

            return destructable;
        }

        private void NotifyHitDestructable(IDestruct destructable, DestructParams destructParams, float dt)
        {
            if (destructable == null)
                return;
            if (!_destructors.TryGetValue(destructable, out var destructor))
            {
                destructor = new Destructor(destructable, this);
                _destructors.Add(destructable, destructor);
            }

            destructor.OnHit(dt, destructParams);
        }


        #region [Editor Gizmos]



        #endregion
    }
}