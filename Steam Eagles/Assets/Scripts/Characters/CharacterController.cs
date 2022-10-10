using System;
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

    #region Private variables

    private GroundCheck _groundCheck;

    private Vector2 _interactPosition;
    private Vector2 _interactVelocity;

    private float _jumpTimeCounter;
    private Rigidbody2D _rigidbody;
    private bool _startedInteractionGrounded;

    private Vector2 lastFramePosition;
    private Vector2 lastFrameDelta;
    private bool wasInteracting;

    private CharacterState State { get; set; }


    private bool JumpPressed => State.JumpPressed;
    private bool IsGrounded => _groundCheck.IsGrounded;
    private bool IsInteracting => State.IsInteracting;
    private bool JumpHeld => State.JumpHeld;
    
    private bool IsJumping
    {
        get => State.IsJumping;
        set => State.IsJumping = value;
    }

    private Rigidbody2D rb => State.Rigidbody;

    private Vector2 AnimatorDelta => State.AnimatorDelta;
    private Health _health;

  
    
    #endregion

    #region [Unity events]

    private void Awake()
    {
        _health = GetComponent<Health>();
        State = GetComponent<CharacterState>();
        _groundCheck = GetComponent<GroundCheck>();
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        State.IsInteractingStream
            .Where(t => t)
            .Subscribe(_ =>
            {
                _startedInteractionGrounded = IsGrounded;
                _interactPosition = transform.position;
            });
    }


    private void FixedUpdate()
    {
        if (State.IsDead) return;
        
        if (IsInteracting) {
            DoForces();
            HandleInteractionFixedUpdate(Time.fixedDeltaTime);
            return;
        }

       

        if (HasGroundedMovement())
            DoGroundedMovement();
        else
            DoAerialMovement();
        
        DoForces();
    }

    private void DoForces()
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
                    rb.velocity += force;
                }
            }
        }
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

    #endregion

    #region [Methods]

    private bool CheckIsGrounded()
    {
        if (IsJumping ) return false;
        return State.IsGrounded && !(State.VelocityY > 0);
    }

    private void DoAerialMovement()
    {
        State.VelocityX = State.MoveX * moveSpeed * Time.fixedDeltaTime;
    }

    private void DoGroundedMovement()
    {
        var normal = Vector2.up;
        var tangent = Vector3.Cross(normal, Vector3.forward);
        var newVelocity = State.MoveX * moveSpeed * Time.fixedDeltaTime * tangent.normalized;
        rb.velocity = newVelocity;
    }

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
        if (IsGrounded && JumpPressed) {
            IsJumping = true;
            _jumpTimeCounter = jumpTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (IsJumping) {
            if (JumpHeld && _jumpTimeCounter > 0.0f) {
                rb.velocity = Vector2.up * jumpForce;
                _jumpTimeCounter -= Time.deltaTime;
            }
            else {
                IsJumping = false;
            }
        }
    }

    private bool HasGroundedMovement()
    {
        return IsGrounded && !IsJumping;
    }

    #endregion
}