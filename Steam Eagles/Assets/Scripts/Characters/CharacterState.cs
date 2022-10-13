using System;
using System.Collections.Generic;
using DefaultNamespace;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(GroundCheck))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterState : MonoBehaviour
{

    #region Public variables

    public CharacterConfig config;

    public float MoveX { get; set; }

    public float MoveY { get; set; }

    public bool JumpPressed { get; set; }

    public bool AttackPressed { get; set; }

    public bool JumpHeld { get; set; }

    public Vector2 AnimatorDelta { get; set; }

    public bool IsJumping { get; set; }

    /// <summary>
    /// is the player currently trying to drop through 1-way platforms 
    /// </summary>
    public bool IsDropping { get; set; }

    public bool IsDead { get; set; }

    public InteractionPhysicsMode InteractionPhysicsMode { get; set; }

    public Rigidbody2D Rigidbody { get; private set; }


    public Animator Animator { get; set; }


    public bool IsInteracting
    {
        get => _isInteractingProperty.Value;
        set => _isInteractingProperty.Value = value;
    }

    public IObservable<bool> IsInteractingStream => _isInteractingProperty;

    public bool IsGrounded
    {
        get => _isGroundedProperty.Value || alwaysGrounded;
        set => _isGroundedProperty.Value = value;
    }


    public IObservable<bool> IsGroundedEventStream => !alwaysGrounded ? _isGroundedProperty : Observable.Return(true);


    public float VelocityX
    {
        get => Rigidbody.velocity.x;
        set => Rigidbody.velocity = new Vector2(value, Rigidbody.velocity.y);
    }

    public float VelocityY
    {
        get => Rigidbody.velocity.y;
        set => Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, value);
    }

    #endregion

    #region Private variables

    public bool alwaysGrounded = false;

    private readonly BoolReactiveProperty _isGroundedProperty = new BoolReactiveProperty(false);

    private readonly BoolReactiveProperty _isInteractingProperty = new BoolReactiveProperty(false);
    public List<Vector4> Forces { get; set; }
    public Vector2 AnimatorAccel { get; set; }
    public bool StunLocked { get; set; }

    #endregion

    #region [Unity events]

    public void Awake()
    {
        Forces = new List<Vector4>();
        Rigidbody = GetComponent<Rigidbody2D>();
        Animator = GetComponentInChildren<Animator>();
        JumpHeld = false;
        JumpPressed = false;
        VelocityX = 0;
        VelocityY = 0;
    }

    #endregion

    public void AddForce(Vector2 force, float duration)
    {
        var f = new Vector4(force.x, force.y, duration, Time.time);
    }

    #region [Methods]

    public void PlayAnimation(string animationName)
    {
        Animator.Play(animationName);
    }

    #endregion


    

}

public enum InteractionPhysicsMode
{
    Default = 0,  //default, root motion controls character's movement
    Mixed = 1,      //root motion is added with gravity
    FullPhysics = 2 //root motion is applied as a force
}