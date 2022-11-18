using System;
using System.Collections;
using Spine;
using System.Collections.Generic;
using System.ComponentModel;
using CoreLib;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;


[RequireComponent(typeof(SkeletonAnimation))]
public class CharacterSkeletonAnimator : MonoBehaviour
{
    private SkeletonAnimation _spineAnimation;
    private AnimationState _animationState;
    private Spine.Skeleton _skeleton;
    private CharacterState _characterState;
    private Collider2D _collider;
    List<ContactPoint2D> _contactPoint2Ds;
    public SharedBool isFacingRight;
    public enum States {IDLE, RUN, JUMP}

    private States _current;
    public States Current
    {
        get => _current;
        set
        {
            if (_current != value)
            {
                _current = value;

                switch (value)
                {
                    case States.IDLE:
                        _animationState.SetAnimation(0, animIdle, true);
                        break;
                    case States.RUN:
                        _animationState.SetAnimation(0, animRun, true);
                        break;
                    case States.JUMP:
                        _animationState.SetAnimation(0, animJump, false);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }

                UpdateState?.Invoke(value);
                _animationState.Apply(_skeleton);
            }
        }
    }

    public UnityEvent<States> UpdateState;
    
    public string animIdle, animRun, animJump;

    private void Awake()
    {
        _contactPoint2Ds = new List<ContactPoint2D>();
    }
    
    

    void Start()
    {
        this._spineAnimation = GetComponent<Spine.Unity.SkeletonAnimation>();
        this._animationState = _spineAnimation.AnimationState;
        this._skeleton = _spineAnimation.Skeleton;
        this._characterState = GetComponentInParent<CharacterState>();
        Debug.Assert(_characterState!=null, this);
        _collider = _characterState.GetComponent<Collider2D>();
    }

    void Update()
    {
        int contactCount = _collider.GetContacts(_contactPoint2Ds);
        bool isGrounded =false;
        if (contactCount > 0)
        {
            foreach (var contact in _contactPoint2Ds)
            {
                if (Vector2.Dot(contact.normal, Vector2.up) > 0)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        else
        {
            isGrounded = _characterState.IsGrounded;
        }

        Vector2 moveInput = new Vector2(_characterState.MoveX, _characterState.MoveY);
        Vector2 velocity = new Vector2(_characterState.VelocityX, _characterState.VelocityY);
        bool isJumping = _characterState.JumpHeld;
        
        SetAnimation(moveInput, velocity, isJumping, isGrounded);
    }

    private void SetAnimation(Vector2 moveInput, Vector2 velocity, bool isJumping, bool isGrounded)
    {
        if (isGrounded)
        {
            float speed = Mathf.Abs(moveInput.x);
            if (speed > 0)
            {
                bool facingRight = moveInput.x > 0;
                _skeleton.ScaleX = facingRight ? 1 : -1;
                if(isFacingRight!=null)isFacingRight.Value = facingRight;
            }
            
            if (speed > 0.1f)
            {
                Current = States.RUN;

            }
            else
            {
                Current = States.IDLE;
            }
        }
        else
        {
            Current = States.JUMP;
        }
        
    }
}
