using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(CharacterState))]
public class CharacterController : MonoBehaviour
{
    #region Public variables

    public float jumpForce = 25;
    public float jumpTime = 1;
    public float moveSpeed = 15f;

    #endregion

   
    private Health _health;
    private GroundCheck _groundCheck;
    private CharacterState _state;

    private Vector2 _interactPosition;
    private Vector2 _interactVelocity;

    private float _jumpTimeCounter;
    
    private bool _startedInteractionGrounded;

    private Vector2 lastFramePosition;
    private Vector2 lastFrameDelta;
    private bool wasInteracting;
    #region Private variables
    private CharacterState State
    {
        get => _state;
        set => _state = value;
    }


    private bool JumpPressed => State.JumpPressed;
    private bool IsGrounded => _groundCheck.IsGrounded;
    private bool IsInteracting => State.IsInteracting;
    private bool JumpHeld => State.JumpHeld;
    private GroundCheck GroundCheck => _groundCheck;
    
    private bool IsJumping
    {
        get => State.IsJumping;
        set => State.IsJumping = value;
    }

    private Rigidbody2D rb => State.Rigidbody;

    private Vector2 AnimatorDelta => State.AnimatorDelta;
   


    public float JumpTime
    {
        get
        {
            if (State.config == null)
            {
                return jumpTime;
            }
            return State.config.jumpTime;
        }
    }
    public float JumpForce
    {
        get
        {
            if (State.config == null)
            {
                return jumpForce;
            }

            return State.config.jumpForce;
        }
    }

    public float MoveSpeed
    {
        get
        {
            if (State.config == null)
            {
                return moveSpeed;
            }
            return State.config.moveSpeed;
        }
    }

    #endregion

    #region [Unity events]

    private void Awake()
    {
        _health = GetComponent<Health>();
        State = GetComponent<CharacterState>();
        _groundCheck = GetComponent<GroundCheck>();
        contactPoint2Ds = new List<ContactPoint2D>(100);
    }

    private void Start()
    {
        State.IsInteractingStream
            .Where(t => t)
            .Subscribe(_ =>
            {
                _startedInteractionGrounded = IsGrounded;
                _interactPosition = transform.position;
                _interactVelocity = rb.velocity;
            }).AddTo(this);
    }

    private void Update()
    {
        State.IsGrounded = CheckIsGrounded();
        State.IsDead = _health.IsDead;
        if (State.IsDead) return;
        if (IsInteracting) {
            HandleInteractionUpdate(Time.deltaTime);
            return;
        }
        HandleJump();
    }

    bool CheckForWater()
    {
        Vector2 pos = rb.position;
        LayerMask waterLayers = LayerMask.GetMask("Water");
        return Physics2D.OverlapPoint(pos, waterLayers) != null;
    }

    private void FixedUpdate()
    {
        if (State.IsDead) return;
        if (State.toAttach != null)
        {
            State.AttachedBody = State.toAttach;
            State.toAttach = null;
        }
        
        if (State.CheckAttached())
        {
            HandleAttachedBody();
            return;
        }
        else
        {
            
        }
        if (IsInteracting) 
        {
            HandleInteractionFixedUpdate(Time.fixedDeltaTime);
            return;
        }
        SlopeCheckVertical();
        ApplyMovement();
    }

    private void HandleAttachedBody()
    {
        
    }

    void ApplyMovement()
    {
        newVelocity.Set(State.MoveX * moveSpeed, State.VelocityY);
        State.Velocity = newVelocity;
        
        if (IsGrounded && !isOnSlope)
        {
            newVelocity.Set(State.MoveX * MoveSpeed, IsJumping ? State.VelocityY : 0);
            DoExternalForces(ref newVelocity);
            State.Velocity = newVelocity;
        }
        else if (IsGrounded && isOnSlope && IsSlopeWalkable())
        {
            float xComponent = MoveSpeed * slopeNormalPerp.x * -State.MoveX;
            float yComponent = MoveSpeed * slopeNormalPerp.y * -State.MoveX;
            newVelocity.Set(xComponent, yComponent);
            DoExternalForces(ref newVelocity);
            State.Velocity = newVelocity;
        }
        else if (!IsGrounded)
        {
            newVelocity.Set(State.MoveX * MoveSpeed, State.VelocityY + State.LiftForce);
            State.Velocity = newVelocity;
        }
}

    private bool IsSlopeWalkable()
    {
        return this.slopeDownAngle <= State.config.maxSlopeAngle;
    }


    void DoExternalForces(ref Vector2 currentVelocity)
    {
        HashSet<Rigidbody2D> detected = new HashSet<Rigidbody2D>();
        var groundHit = GroundCheck.Hit;
        var hitColl = groundHit.collider;
        if (hitColl.attachedRigidbody != null)
        {
            detected.Add(hitColl.attachedRigidbody);
            CheckCollider(hitColl, groundHit.normal, ref currentVelocity);
        }
        
        
        var count = State.Rigidbody.GetContacts(contactPoint2Ds);
        for (int i = 0; i < count; i++)
        {
            var contactPoint = contactPoint2Ds[i];
            var coll = contactPoint.collider;
            if(!coll.attachedRigidbody || detected.Contains(coll.attachedRigidbody)) continue;
            
            if (CheckCollider(coll, contactPoint.normal, ref currentVelocity)) 
                detected.Add(coll.attachedRigidbody);
        }
        
    }

    private static bool CheckCollider(Collider2D coll, Vector2 normal, ref Vector2 currentVelocity)
    {
        
        if (coll.attachedRigidbody != null && coll.gameObject.CompareTag("Moving Platform"))
        {
            var contactPointNormal = normal;
            if (Vector2.Dot(contactPointNormal, Vector2.up) > 0.1f)
            {
                var movingPlatformVelocity = coll.attachedRigidbody.velocity;
                currentVelocity += movingPlatformVelocity;
                return true;
            }
        }

        return false;
    }

    private Vector2 newVelocity;
    private Vector2 slopeNormalPerp;
    private float slopeDownAngle;
    private float slopeDownAngleOld;

    private bool isOnSlope;

    private void SlopeCheckVertical()
    {
        if (IsGrounded) SlopeCheckVertical(GroundCheck.Hit);
    }

    private void SlopeCheckVertical(RaycastHit2D hit)
    {
        if (hit) SlopeCheckVertical(hit.normal);
    }
    private void SlopeCheckVertical(Vector2 groundNormal)
    {
        slopeNormalPerp = Vector2.Perpendicular(groundNormal).normalized;
        slopeDownAngle = Vector2.Angle(groundNormal, Vector2.up);
        if (slopeDownAngleOld != slopeDownAngle)
        {
            isOnSlope = true;
        }
        slopeDownAngleOld = slopeDownAngle;
        Debug.DrawRay(transform.position, slopeNormalPerp*5, Color.magenta);
    }
    private void DoGroundedMovement()
    {
        Vector2 DoExternalForces(Vector2 vector2)
        {
            int count = State.Rigidbody.GetContacts(contactPoint2Ds);
            for (int i = 0; i < count; i++)
            {
                var contactPoint = contactPoint2Ds[i];
                if (contactPoint.collider.attachedRigidbody != null && contactPoint.collider.gameObject.CompareTag("Moving Platform"))
                {
                    var contactPointNormal = contactPoint.normal;
                    if (Vector2.Dot(contactPointNormal, Vector2.up) > 0.1f)
                    {
                        var movingPlatformVelocity = contactPoint.collider.attachedRigidbody.velocity;
                        vector2 += movingPlatformVelocity * Time.fixedDeltaTime;
                    }
                }
            }

            return vector2;
        }

        Vector2 externalForces=Vector2.zero; 
        
        externalForces = DoExternalForces(externalForces);
        var normal = Vector2.up;
        var tangent = Vector3.Cross(normal, Vector3.forward);
     
        newVelocity = State.MoveX * moveSpeed * Time.fixedDeltaTime * tangent.normalized;
        rb.velocity = newVelocity + externalForces;
    }

    private void DoForces(ref Vector2 velocity)
    {
        if (State.Forces.Count > 0)
        {
            foreach (var forceData in State.Forces)
            {
                float forceStartTime = forceData.w;
                float forceDuration = forceData.z;
                Vector2 force = new Vector2(forceData.x, forceData.y);
                if ((Time.time - forceStartTime) < forceDuration)
                {
                    Debug.DrawRay(transform.position, force, Color.cyan, 1);
                    velocity += force;
                }
            }
        }
    }

    #endregion

    #region [Methods]

    private bool CheckIsGrounded()
    {
        if (IsJumping ) return false;
        return State.IsGrounded && !(State.VelocityY > 0);
    }

    private void DoAerialMovement(ref Vector2 velocity)
    {
        velocity.Set(State.MoveX * moveSpeed * Time.fixedDeltaTime, velocity.y);
    }

    List<ContactPoint2D> contactPoint2Ds = new List<ContactPoint2D>();
    private Collider2D _collider;
    private Collider2D collider => _collider == null ? (_collider = GetComponent<Collider2D>()) : _collider;


    private void HandleInteractionFixedUpdate(float dt)
    {
        switch (State.InteractionPhysicsMode) {
            case InteractionPhysicsMode.Default:
            case InteractionPhysicsMode.FullPhysics:
                return;
            case InteractionPhysicsMode.Mixed:
                var lastVelocity = rb.velocity;
                lastVelocity += (Physics2D.gravity*dt);
                lastVelocity += AnimatorDelta;
                rb.velocity = lastVelocity;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void HandleInteractionUpdate(float dt)
    {
        IsJumping = false;
        switch (State.InteractionPhysicsMode) {
            case InteractionPhysicsMode.Default:
                rb.MovePosition(rb.position + AnimatorDelta);
                break;
            case InteractionPhysicsMode.Mixed:
                break;
            case InteractionPhysicsMode.FullPhysics:
                rb.AddForce(State.AnimatorDelta);
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    private void HandleJump()
    {
        if (Time.time - lastDropTime < 0.5f) return;
        if (CheckForDropThroughPlatform()) return;
        if ((IsGrounded || CheckForWater()) && JumpPressed) {
            IsJumping = true;
            _jumpTimeCounter = jumpTime;
            rb.velocity = Vector2.up * jumpForce;
            State.onJumped.OnNext(Unit.Default);
        }

        if (IsJumping) {
            if (JumpHeld && _jumpTimeCounter > 0.0f) {
                rb.velocity = Vector2.up * (JumpForce + State.ExtraJumpForce);
                _jumpTimeCounter -= Time.deltaTime;
                if (State.ExtraJumpTime > 0) State.ExtraJumpTime = Mathf.Max(0, State.ExtraJumpTime - Time.deltaTime);
                
            }
            else {
                IsJumping = false;
            }
        }
    }

    private bool CheckForDropThroughPlatform()
    {
        if (JumpHeld && (State.MoveY < 0 && Mathf.Abs(State.MoveY) > Mathf.Abs(State.MoveX)))
        {
            Debug.DrawRay(transform.position, Vector3.down * 2f, Color.green, 1);

            if (Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("Pipes", "Platforms")))
            {
                StartCoroutine(DropThroughPlatform());
                return true;
            }
        }

        return false;
    }

    private float lastDropTime;
    
    IEnumerator DropThroughPlatform()
    {
        
        var collider = GetComponent<Collider2D>();
        var coll = _groundCheck.Hit.collider;
        if (coll == null)
        {
            Debug.LogError("HIT FOUND NO COLLIDER");
            yield break;
        }

        lastDropTime = Time.time;
        State.VelocityY = -5;
        
        
        Physics2D.IgnoreCollision(collider, coll, true);
        State.IsDropping = true;
        yield return new WaitForSeconds(0.25f);
        State.IsDropping = false;
        Physics2D.IgnoreCollision(collider, coll, false);
        
        
    }

    private bool HasGroundedMovement()
    {
        return IsGrounded && !IsJumping;
    }
    
    #endregion
}