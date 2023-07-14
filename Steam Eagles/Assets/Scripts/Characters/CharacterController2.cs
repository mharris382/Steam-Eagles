using System;
using System.Collections;
using CoreLib;
using DefaultNamespace;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using StateMachine = FSM.StateMachine;

namespace Characters
{
    [RequireComponent(typeof(StructureState))]
    public class CharacterController2 : MonoBehaviour
    {
        private const string CHARACTER_CONFIG_ADDRESS = "CharacterConfig";

        #region [Fields]

        [SerializeField] private float slopeCheckDistance =1;
        public bool NormalizeXInput = true;
        
        
        public Transform groundCheck;
        private CharacterState _state;
        private Rigidbody2D _rigidbody2D;
        private CharacterInputState _input;
        private CapsuleCollider2D _capsuleCollider;
        private PhysicsMaterial2D _fullFriction;
        private PhysicsMaterial2D _noFriction;
        private PhysicsMaterial2D _walkingFriction;
        private AsyncOperationHandle<CharacterConfig> _configLoadHandle;


        private bool _canJumpBecauseGrounded;
        private Vector2 _slopeNormalPerp;
        private float _slopeDownAngle;
        private float _slopeDownAngleOld;
        private bool _isOnSlope;
        private float _slopSideAngle;


        private bool __isGrounded;
        private bool __isJumping;
        private bool __facingRight = true;
        private bool __isDropping;
        private bool __inWater;
        private bool _onBalloon;
        private bool _onOneWayPlatform;

        private bool _canWalkOnSlope;
        private bool _wasJumping;
        private bool _wasOnBalloon;
        private bool _isBalloonJumping;
        private float _timeOnBalloon;

        private Vector2 _newVelocity;
        private float _lastDropTime;
        private float _jumpTimeCounter;
        private ContactPoint2D[] _contactPoint2Ds = new ContactPoint2D[10];

      
        private StructureState _structureState;
        
      
        
        private Collider2D _onOneWay;
        private Collider2D[] _oneWayColliders = new Collider2D[10];
        private Collider2D[] _triggerColliders = new Collider2D[10];
        private Collider2D[] _droppingColliders = new Collider2D[10];
        private RaycastHit2D[] _oneWayHits = new RaycastHit2D[10];
        private RaycastHit2D[] _triggerHits = new RaycastHit2D[10];

        private RaycastHit2D _balloonHit
        {
            get => State._balloonHit;
            set => State._balloonHit = value;
        }

        public bool IsBalloonJumping => _isBalloonJumping;

        public bool ForceDisableBuildingAttach { get; set; }
        
        public bool IsAttachedToBuilding => buildingRigidbody != null && buildingJoint.enabled ;
        
        private Collider2D __balloonCollider;

        #endregion

        #region [Properties]

        #region [Private]
        
        private FixedJoint2D buildingJoint
        {
            get => _structureState.BuildingJoint;
        }

        private Rigidbody2D buildingRigidbody
        {
            get => _structureState.BuildingRigidbody;
            set => _structureState.BuildingRigidbody = value;
        }

        private Rigidbody2D platformRigidbody
        {
            get => _structureState.PlatformRigidbody;
            set => _structureState.PlatformRigidbody = value;
        }

        #endregion
        
        public bool OnBalloon
        {
            get => _onBalloon;
            set
            {
                _onBalloon = value;
                if (value && !IsGrounded)
                {
                    _wasOnBalloon = true;
                    _timeOnBalloon = Time.time;
                }

                if (!value && _wasOnBalloon)
                {
                    if (Time.time - _timeOnBalloon > Config.balloonJumpCoyoteTime)
                    {
                        BalloonCollider = null;
                        _wasOnBalloon = false;
                    }
                }
            }
        }

        
        private bool IsGrounded
        {
            get => __isGrounded;
            set
            {
                __isGrounded = value;
                _state.IsGrounded = _state.IsOnSolidGround = value;
            }
        }
        
        private bool _isJumping
        {
            get => __isJumping;
            set
            {
                __isJumping = value;
                _state.IsJumping = value;
            }
        }
        
        private bool _isDropping
        {
            get => __isDropping;
            set
            {
                __isDropping = value;
                _state.IsDropping = value;
            }
        }
        
        private bool _inWater
        {
            get => __inWater;
            set
            {
                __inWater = value;
                _state.InWater = value;
            }
        }

        public bool _facingRight
        {
            get => __facingRight;
            set
            {
                __facingRight = value;
                _state.FacingRight = value;
            }
        }
        
        public Collider2D BalloonCollider
        {
            get => _wasOnBalloon ?  __balloonCollider : null;
            set
            {
                if (value != null)
                {
                    __balloonCollider = value;
                }
            }
        }

        private CharacterState State => _state != null ? _state : (_state = GetComponent<CharacterState>());
        public CharacterConfig Config => State.config;
        public Rigidbody2D rb => _rigidbody2D;
        public float xInput => (NormalizeXInput ? (Mathf.Abs(State.MoveX) > 0.1f ? Mathf.Sign(State.MoveX) : 0) : State.MoveX);
        public float yInput => State.MoveY;
        public LayerMask whatIsGround => Config.GetGroundLayers();
        public float MoveSpeed => State.GetMoveSpeed();
        public float JumpForce => Config.jumpForce;

        public Vector2 colliderSize => _capsuleCollider.size;

        
        public float MoveDirection => Mathf.Sign(xInput);
        public PhysicsMaterial2D FrictionlessPhysicsMaterial => _noFriction;
        public PhysicsMaterial2D FullFrictionPhysicsMaterial => _fullFriction;

        public Collider2D OneWayCollider => _onOneWay;

      
        #endregion

        #region [Unity Methods]

        private void Awake()
        {
            if (!gameObject.TryGetComponent(out _structureState ))
            {
                _structureState = gameObject.AddComponent<StructureState>();
            }
            
            _state = GetComponent<CharacterState>();
            _state.resetJumpTimer = ResetJumpTimer;
            _input = GetComponent<CharacterInputState>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            Debug.Assert(State.config != null, "NO CHARACTER CONFIG", this);
            _fullFriction = State.config.GetFullFrictionMaterial();
            _noFriction = State.config.GetNoFrictionMaterial();
            _walkingFriction = State.config.GetWalkingFrictionMaterial();
        }

        #endregion

        void ResetJumpTimer()
        {
            _jumpTimeCounter = State.jumpTime;
        }
        public void ApplyMovement(float dt)
        {
            if (State.IsDropping)
            {
                StopDropping();
            }

            if (IsGrounded && !_isOnSlope && !_isJumping && !State.IsDropping)
            {
                _newVelocity.Set(MoveSpeed * xInput, 0.0f);
            }
            else if (IsGrounded && _isOnSlope && !_isJumping && !State.IsDropping)
            {
                var moveSpeed = MoveSpeed;
              
              
                _newVelocity.Set(moveSpeed * _slopeNormalPerp.x * -xInput, moveSpeed * _slopeNormalPerp.y * -xInput);
                _newVelocity = _newVelocity.normalized * MoveSpeed;

            }
            else if(!IsGrounded)
            {
                _newVelocity.Set(MoveSpeed * xInput, rb.velocity.y);
            }
            ApplyVelocity(dt);
        }

        private void ApplyVelocity(float dt)
        {
            if (IsAttachedToBuilding)
            {
                buildingJoint.ApplyVelocityViaConnectedAnchor(_newVelocity, dt);
                return;
            }
            rb.velocity = _newVelocity;
        }

        public void ApplyHorizontalMovement(float dt, float multiplier = 1)
        {
            _newVelocity.Set(MoveSpeed * xInput * multiplier, rb.velocity.y);
            if (IsAttachedToBuilding)
            {
                return;                
            }
            
            rb.velocity = _newVelocity;    
            
        }

        public void ApplyJumpForce()
        {
            if (_isJumping)
            {
                if (CheckForCeiling())
                {
                    _jumpTimeCounter = 0;
                }
                if (_jumpTimeCounter > 0)
                {
                    float jumpForce = Config.jumpForce;
                    if(_isBalloonJumping)jumpForce *= State.GetBalloonJumpMultiplier();
                    
                    float t = _jumpTimeCounter / State.jumpTime;
                    
                    var x = rb.velocity.x;
                    rb.velocity = new Vector2(x, jumpForce);
                }
            }
        }

        public void UpdateGround()
        {
            var queriesHitTriggersPrev = Physics2D.queriesHitTriggers; 
            var groundLayers = LayerMask.GetMask("Ground", "Solids");
            var balloonLayers = LayerMask.GetMask("Balloons");
            var oneWayLayer = LayerMask.GetMask("Platforms", "Pipes");
            var buildingLayer = LayerMask.GetMask("Triggers");
            
            var pos = groundCheck.position;
            var radius = Config.groundCheckRadius;
            
            Physics2D.queriesHitTriggers = false;
            Physics2D.queriesStartInColliders = false;
            var groundHit = Physics2D.Raycast(pos, Vector2.down, radius, groundLayers);
            var balloonHit = Physics2D.Raycast(pos, Vector2.down, radius, balloonLayers);
            var onGround = groundHit ? groundHit.collider : null; // Physics2D.OverlapCircle(pos, radius, groundLayers);
            var onBalloon = balloonHit ? balloonHit.collider : null;//Physics2D.OverlapCircle(pos, radius, balloonLayers);
            this.OnBalloon = onBalloon != null;
            if (OnBalloon && Config.alwaysJumpOnBalloons) State.IsJumping = true;
            BalloonCollider = onBalloon;
            _balloonHit = balloonHit;

            var oneWayHit = Physics2D.Raycast(pos, Vector2.down, radius, oneWayLayer);
            _onOneWay = oneWayHit ? oneWayHit.collider : null;
            
            
            var cnt = Physics2D.OverlapCircleNonAlloc(pos, radius, _oneWayColliders, oneWayLayer);
            _onOneWayPlatform = cnt > 0;
            
            Physics2D.queriesHitTriggers = true;
            var hits = Physics2D.OverlapCircleNonAlloc(pos, radius, _triggerColliders, buildingLayer);
            
            buildingRigidbody = null;
            platformRigidbody = null;
            for (int i = 0; i < hits; i++)
            {
                if (_triggerColliders[i].gameObject.CompareTag("Building"))
                {
                    buildingRigidbody = _triggerColliders[i].gameObject.GetComponent<Rigidbody2D>();
                    
                }
                else if (_triggerColliders[i].gameObject.CompareTag("Moving Platform"))
                {
                    platformRigidbody = _triggerColliders[i].gameObject.GetComponent<Rigidbody2D>();
                }
            }
            
            IsGrounded = (onGround || _onOneWayPlatform) &&
                         !_isJumping;
            
            if (IsGrounded && rb.velocity.y <= 0.01f)
            {
                _canJumpBecauseGrounded = true;   
            }

            State.IsOnSolidGround = IsGrounded;
            Physics2D.queriesHitTriggers = queriesHitTriggersPrev;
        }


        public void CheckJumping()
        {
            if (!_isJumping)
            {
                if ((_canJumpBecauseGrounded || _inWater) && State.JumpPressed)
                {
                    _isJumping = true;
                    _canJumpBecauseGrounded = false;
                    _jumpTimeCounter = State.jumpTime;
                }
            }
            else if (!State.JumpHeld)
            {
                _jumpTimeCounter = 0;
                _isJumping = false;
            }
        }

        

        public void UpdateFacingDirection()
        {
            if (Mathf.Abs(xInput) > 0.1f)
            {
                _facingRight = xInput > 0;
                State.FacingRight = _facingRight;
            }
        }

        public void UpdateSlopes()
        {
            Vector2 checkPos = transform.position - new Vector3(0.0f, colliderSize.y/2f, 0.0f);
            SlopeCheckHorizontal(checkPos);
            
            SlopeCheckVertical(checkPos);
            CheckSlopeWalkable();
        }

        public void UpdatePhysMat()
        {
            if (!IsGrounded)
            {
                SetPhysicsMaterial(FrictionlessPhysicsMaterial);
            }
            else if (Mathf.Abs(xInput) < 0.1f)
            {
                SetPhysicsMaterial(FullFrictionPhysicsMaterial);
            }
            else
            {
                SetPhysicsMaterial(_walkingFriction);
            }
        }

        public void SetPhysicsMaterial(PhysicsMaterial2D physicsMaterial2D)
        {
            _capsuleCollider.sharedMaterial = physicsMaterial2D;
        }

        public void UpdateJumpTimer(float dt)
        {
            if (_isJumping)
            {
                if (_jumpTimeCounter > 0 && State.JumpHeld)
                {
                    _jumpTimeCounter -= dt;
                }
                else
                {
                    _jumpTimeCounter = 0;
                    _isJumping = false;
                }
            }
        }

        public void CheckWater()
        {
            Vector2 pos = rb.position;
            pos.y -= colliderSize.y / 2f;
            LayerMask waterLayers = LayerMask.GetMask("Water");
            _inWater = Physics2D.OverlapPoint(pos, waterLayers) != null;
        }

        public bool AbleToDrop()
        {
            if (!IsGrounded)
            {
                return false;
            }
            if (Time.time - _lastDropTime < 0.5f)
            {
                return false;
            }
            return _onOneWay;
        }

        public bool AbleToJump()
        {
            if (IsGrounded || _inWater || _wasOnBalloon || State.WasClimbingRecently)
            {
                return true;
            }
            return false;
        }
        

        public void BeginJump()
        {
            if (_wasOnBalloon)
            {
                _isBalloonJumping = true;
            }
            _isJumping = true;
            State.IsJumping = true;
            _jumpTimeCounter = State.jumpTime;
        }
        public void EndJump()
        {
            _isBalloonJumping = false;
            _isJumping = false;
            State.IsJumping = false;
            _jumpTimeCounter = 0;
        }
        
        public void BeginDropping()
        {
            if (_droppingColliders != null) 
                StopDropping();
            
            if (!AbleToDrop())
            {
                Debug.LogError($"{name} Cannot Drop because not grounded, not on one way platform or not enough time has passed since last drop", this);
                return;
            }
            
            State.IsDropping = true;
            _lastDropTime = Time.time;
            _droppingColliders = _oneWayColliders;
            foreach (var oneWayCollider in _droppingColliders)
            {
                if (oneWayCollider == null) continue;
                Physics2D.IgnoreCollision(_capsuleCollider, oneWayCollider, true);
            }
        }

        public void StopDropping()
        {
            State.IsDropping = false;
            foreach (var droppingCollider in _droppingColliders)
            {
                if (droppingCollider == null) continue;
                Physics2D.IgnoreCollision(_capsuleCollider, droppingCollider, false);
            }
            _droppingColliders = null;
        }

        public bool CheckForCeiling()
        {
            int contacts = rb.GetContacts(_contactPoint2Ds);
            for (int i = 0; i < contacts; i++)
            {
                var contactPoint = _contactPoint2Ds[i];
                if (contactPoint.normal.y < -0.1f && contactPoint.point.y > transform.position.y)
                {
                    return true;
                }
            }

            return false;
        }

        private void SlopeCheckHorizontal(Vector2 checkPos)
        {
            var offset = Vector2.up * 0.25f;
            RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos + offset, _facingRight ? Vector2.right : Vector2.left, slopeCheckDistance, whatIsGround);
            RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos + offset,_facingRight ? Vector2.left : Vector2.right, slopeCheckDistance, whatIsGround);
            Debug.DrawRay(checkPos+ offset, _facingRight ? Vector2.right * slopeCheckDistance : Vector2.left * slopeCheckDistance, Color.red);
            Debug.DrawRay(checkPos+ offset, _facingRight ? Vector2.left * slopeCheckDistance : Vector2.right * slopeCheckDistance, Color.blue);
            if (slopeHitBack)
            {
                _isOnSlope = true;
                _slopSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
            }
            else if (slopeHitBack)
            {
                _isOnSlope = true;
                _slopSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
            }
            else
            {
                _isOnSlope = false;
                _slopSideAngle = 0;
            }
        }

        private void SlopeCheckVertical(Vector2 checkPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance, whatIsGround);
            if (!hit)
            {
                return;
            }
            var groundNormal = hit.normal; 
            this._slopeNormalPerp = Vector2.Perpendicular(groundNormal).normalized;
            this._slopeDownAngle = Vector2.Angle(groundNormal, Vector2.up);
            if (Math.Abs(this._slopeDownAngleOld - _slopeDownAngle) > Mathf.Epsilon)
            {
                this._isOnSlope = true;
            }
            _slopeDownAngleOld = _slopeDownAngle;
            Debug.DrawRay(hit.point, hit.normal, Color.red);
            Debug.DrawRay(transform.position, _slopeNormalPerp*5, Color.magenta);
        }

        void CheckSlopeWalkable()
        {
            if (_slopeDownAngle > Config.maxSlopeAngle || _slopSideAngle > Config.maxSlopeAngle)
            {
                _canWalkOnSlope = false;
            }
            else
            {
                _canWalkOnSlope = true;
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (Config != null)
            {
                var color = IsGrounded ? Color.green : Color.red;
                Gizmos.color = color.SetAlpha(0.8f);
                var pos = groundCheck.position;
                Gizmos.DrawSphere( groundCheck.position, Config.groundCheckRadius);
                if (_onOneWayPlatform)
                {
                    Gizmos.color = Color.blue.SetAlpha(0.8f);
                    pos.z -= Config.groundCheckRadius;
                    Gizmos.DrawSphere(pos, Config.groundCheckRadius/2f);
                }
            }
        }

        [System.Obsolete]
        public void ClearParent()
        {
           // transform.SetParent(null);
        }

        
        /// <summary>
        /// checks to see if the player should be parented to the building they are inside
        /// </summary>
        [System.Obsolete]
        public void CheckParent()
        {
            return;
            
        }
    }
}