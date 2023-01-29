using System;
using System.Collections;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using StateMachine = FSM.StateMachine;

namespace Characters
{
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


        private bool _isGrounded;
        private bool _canJumpBecauseGrounded;
        private Vector2 _slopeNormalPerp;
        private float _slopeDownAngle;
        private float _slopeDownAngleOld;
        private bool _isOnSlope;
        private float _slopSideAngle;
        private bool _isJumping;
        private Vector2 _newVelocity;
        private bool _canWalkOnSlope;
        private bool _facingRight = true;
        private float _lastDropTime;
        private bool _isDropping;
        private ContactPoint2D[] _contactPoint2Ds = new ContactPoint2D[10];
        private bool _inWater;
        private bool _wasJumping;
        private float _jumpTimeCounter;
        private RaycastHit2D _verticalHit;
        private Collider2D _onOneWay;

        #endregion
        #region [Properties]

        private CharacterState State => _state;
        public CharacterConfig Config => State.config;
        public Rigidbody2D rb => _rigidbody2D;
        public float xInput => (NormalizeXInput ? (Mathf.Abs(State.MoveX) > 0.1f ? Mathf.Sign(State.MoveX) : 0) : State.MoveX);
        public LayerMask whatIsGround => Config.GetGroundLayers();
        public float MoveSpeed => Config.moveSpeed;
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
            _state = GetComponent<CharacterState>();
            _input = GetComponent<CharacterInputState>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            
            Debug.Assert(State.config != null, "NO CHARACTER CONFIG", this);
            _fullFriction = State.config.GetFullFrictionMaterial();
            _noFriction = State.config.GetNoFrictionMaterial();
            _walkingFriction = State.config.GetWalkingFrictionMaterial();
        }

#if FALSE
        private void Update()
        {
            UpdateJumpTimer(Time.deltaTime);
            CheckJumping();
        }

        private void FixedUpdate()
        {
            CheckFacingDirection();
            CheckGround();
            CheckWater();
            CheckSlopes();
            UpdatePhysMat();
            ApplyMovement();
            ApplyJumpMovement();
            ApplyGravity();
        }
#endif

        #endregion

        public void ApplyMovement()
        {
            if (_isGrounded && !_isOnSlope && !_isJumping)
            {
                _newVelocity.Set(MoveSpeed * xInput, 0.0f);
            }
            else if (_isGrounded && _isOnSlope && !_isJumping)
            {
                _newVelocity.Set(MoveSpeed * _slopeNormalPerp.x * -xInput, MoveSpeed * _slopeNormalPerp.y * -xInput);
            }
            else if(!_isGrounded)
            {
                _newVelocity.Set(MoveSpeed * xInput, rb.velocity.y);
            }
            rb.velocity = _newVelocity;
        }


        public void ApplyHorizontalMovement()
        {
            _newVelocity.Set(MoveSpeed * xInput, rb.velocity.y);
            rb.velocity = _newVelocity;
        }

        public void ApplyJumpMovement()
        {
            if (_isJumping)
            {
                if (CheckForCeiling()) _jumpTimeCounter = 0;
                if (State.JumpHeld && _jumpTimeCounter > 0)
                {
                    float jumpForce = Config.jumpForce;
                    float t = _jumpTimeCounter / Config.jumpTime;
                    jumpForce *= t;
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                }
            }
        }

        public void ApplyGravity()
        {
            if (_isGrounded && !_verticalHit.rigidbody.isKinematic)
            {
                var forceOfGravity = Physics2D.gravity;
                rb.AddForce(forceOfGravity * rb.mass);
            }
        }

        public void CheckGround()
        {
            //_isGrounded = Physics2D.OverlapCircle(groundCheck.position, Config.groundCheckRadius, whatIsGround);
            var groundLayers = LayerMask.GetMask("Ground", "Solids", "Balloons");
            var oneWayLayer = LayerMask.GetMask("Platforms", "Pipes");
            var pos = groundCheck.position;
            var radius = Config.groundCheckRadius;
            var onGround = Physics2D.OverlapCircle(pos, radius, groundLayers);
            _onOneWay = Physics2D.OverlapCircle(pos, radius, oneWayLayer);
            _isGrounded = onGround || ((_onOneWay != null) && _rigidbody2D.velocity.y <= 0 && !_isJumping);
            if (_isGrounded && rb.velocity.y <= 0.01f)
            {
                _canJumpBecauseGrounded = true;   
            }
        }

        public void CheckJumping()
        {
            if (!_isJumping)
            {
                if (CheckForDrop()) return;
                if ((_canJumpBecauseGrounded || _inWater) && State.JumpPressed)
                {
                    _isJumping = true;
                    _canJumpBecauseGrounded = false;
                    _jumpTimeCounter = Config.jumpTime;
                }
            }
            else if (!State.JumpHeld)
            {
                _jumpTimeCounter = 0;
                _isJumping = false;
            }
        }

        public void CheckFacingDirection()
        {
            if(Mathf.Abs(xInput) > 0.1f)
                _facingRight = xInput > 0;
        }

        public void CheckSlopes()
        {
            Vector2 checkPos = transform.position - new Vector3(0.0f, colliderSize.y/2f, 0.0f);
            SlopeCheckHorizontal(checkPos);
            
            SlopeCheckVertical(checkPos);
            CheckSlopeWalkable();
        }

        public void UpdatePhysMat()
        {
            if (!_isGrounded)
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

        public bool CheckForDrop()
        {
            if (!_isGrounded) return false;
            if (Time.time - _lastDropTime < 0.5f) return false;
            if (State.JumpHeld && State.MoveY < 0 && Mathf.Abs(State.MoveY) > Mathf.Abs(State.MoveX))
            {
                var hit = Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("Pipes", "Platforms"));
                if (hit)
                {
                    _lastDropTime = Time.time;
                    State.IsDropping = true;
                    //StartCoroutine(DropThroughPlatform(hit));
                    return true;
                }
            }
            return false;
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
            
            RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, _facingRight ? Vector2.right : Vector2.left, slopeCheckDistance, whatIsGround);
            RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos,_facingRight ? Vector2.left : Vector2.right, slopeCheckDistance, whatIsGround);
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
            _verticalHit = hit;
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

        IEnumerator DropThroughPlatform(RaycastHit2D hit)
        {
            var col = hit.collider;
            State.VelocityY = -5;
            Physics2D.IgnoreCollision(col, _capsuleCollider, true);
            State.IsDropping = true;
            _isDropping = true;
            yield return new WaitForSeconds(0.25f);
            _isDropping = false;
            State.IsDropping = false;
            Physics2D.IgnoreCollision(col, _capsuleCollider, false);
        }


        
    }
}