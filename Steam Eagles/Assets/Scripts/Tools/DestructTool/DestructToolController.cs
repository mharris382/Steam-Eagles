using System;
using System.Collections.Generic;
using Buildings.Rooms;
using CoreLib;
using CoreLib.Interfaces;
using Sirenix.OdinInspector;
using Tools.BuildTool;
using UniRx;
using UnityEngine;

namespace Tools.DestructTool
{
    public class DestructToolController : ToolControllerBase
    {
        [Min(0)] [SerializeField,HideInInspector]
        private float minDistance = 0.5f;
        
        [Min(0)] [SerializeField,HideInInspector]
        private float maxDistance = 3f;

        [SerializeField] private Vector2 originOffset = new Vector2(0,1);
        [SerializeField] private float aimSpeed = 3f;
        [SerializeField] private float aimSmoothing = 0.1f;
        [SerializeField] private DestructionConfig config;
        public Transform aimPoint;
        private Vector2 _actualAimPositionLs;
        private float _aimVelocity;
        private float _speed;
        

        [ShowInInspector, MinMaxSlider(0, 10)]
        public Vector2 ToolRange
        {
            get => new Vector2(minDistance, maxDistance);
            set
            {
                minDistance = Mathf.Min(value.x, value.y);
                maxDistance = Mathf.Max(value.x, value.y);
            }
        }
        
        [Serializable]
        public class DestructionConfig
        {
            public LayerMask destructibleLayers;// = LayerMask.GetMask("Solids", "Pipes", "Machines");
            public float radius = 1f;
            public float rate = 1f;
            [Tooltip("If the destuctor does not receive a hit for this amount of time, it will reset the time until next destruction")]
            public float destructionResetTimer = 0.5f;
        }
        
        protected override void OnRoomChanged(Room room)
        {
            HasRoom = room != null && room.buildLevel == BuildLevel.FULL;
        }
        
        private void OnDrawGizmosSelected()
        {
            var position = transform.position + (Vector3)originOffset;
            Gizmos.color = Color.red.SetAlpha(0.5f);
            Gizmos.DrawSphere(position, minDistance);
            Gizmos.color = Color.red.SetAlpha(0.1f);
            Gizmos.DrawSphere(position, maxDistance);
        }

        protected override void OnStart()
        {
            aimPoint.parent = transform;
            _actualAimPositionLs = (Vector2)transform.localPosition + originOffset;
        }

        void ClampPosition()
        {
            var position = _actualAimPositionLs;
            var distance = position.sqrMagnitude;
            if (distance < Mathf.Pow(minDistance, 2))
            {
                position = position.normalized * minDistance;
            }
            else if (distance > Mathf.Pow(minDistance, 2))
            {
                position = position.normalized * maxDistance;
            }

            _actualAimPositionLs = position;
            aimPoint.localPosition = position - originOffset;   
        }

        
        private void Update()
        {
            if (!HasResources())
            {
                Debug.Log("No resources",this);
                return;
            }

            UpdateAim(Time.deltaTime);
        }

        private void UpdateAim(float dt)
        {
            var aimInput = CharacterState.Tool.Inputs.AimInputRaw;
            if (aimInput.sqrMagnitude < 0.1f)
            {
                return;
            }
            
            var targetDirection = aimInput.normalized;
            var origin = ((Vector2)transform.localPosition + originOffset);
            var actualDirection = _actualAimPositionLs - origin;
            if(Vector2.Dot(targetDirection, actualDirection) < 0)
                _actualAimPositionLs = origin;
            
            // var targetPosition = origin + aimInput;
            // var actualPosition = _actualAimPositionLs;
            //
            // float desiredSpeed = aimSpeed * ((targetPosition - actualPosition).sqrMagnitude > 0.1f ? 1 : 0);
            // _speed = Mathf.SmoothDamp(_speed, desiredSpeed, ref _aimVelocity, aimSmoothing);
            // _actualAimPositionLs = Vector2.Lerp(_actualAimPositionLs, targetPosition, dt * _speed);
            // ClampPosition();

            _actualAimPositionLs = origin + targetDirection * maxDistance;
            var direction = _actualAimPositionLs - origin;
            aimPoint.localPosition = _actualAimPositionLs;
            var position = aimPoint.position;
            CheckForDestructables(position, direction, Time.deltaTime);
        }


        private RaycastHit2D[] _cache = new RaycastHit2D[30];
        private void CheckForDestructables(Vector2 position, Vector2 direction, float dt)
        {
            
            using (new Physics2DQueryScope(hitTriggers: true, startInColliders: true))
            {
                //int hits = Physics2D.OverlapCircleNonAlloc(position, config.radius, _cache, config.destructibleLayers);
                int hits = Physics2D.CircleCastNonAlloc(position, config.rate, direction, _cache, config.destructibleLayers);
                Debug.DrawRay(position, direction, Color.red, 0.1f);
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
            if(!_destructors.TryGetValue(destructable, out var destructor))
            {
                destructor = new Destructor(destructable, this);
                _destructors.Add(destructable, destructor);
            }
            destructor.OnHit(dt, destructParams);
        }

        
        private Dictionary<Collider2D, IDestruct> _seenDestructables = new Dictionary<Collider2D, IDestruct>();
        private Dictionary<IDestruct, Destructor> _destructors = new Dictionary<IDestruct, Destructor>();
        
        public class Destructor
        {
            private readonly IDestruct _destructable;
            private readonly DestructToolController _tool;

            private float _lastHitTime;
            private float _remainingTimeTillNextDestruct;
            public Destructor(IDestruct destructable, DestructToolController tool)
            {
                _destructable = destructable;
                _tool = tool;
                _remainingTimeTillNextDestruct = tool.config.rate;
            }

            public void OnHit(float dt,DestructParams dparams)
            {
                if (Time.realtimeSinceStartup - _lastHitTime > _tool.config.destructionResetTimer)
                    _remainingTimeTillNextDestruct = _tool.config.rate;
                
                _lastHitTime = Time.realtimeSinceStartup;
                _remainingTimeTillNextDestruct -= dt;
                if(_remainingTimeTillNextDestruct > 0)
                    return;
                
                _remainingTimeTillNextDestruct += _tool.config.rate;
                _destructable.TryToDestruct(dparams);
            }
        }
    }
}