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

    private Vector2 _moveInput;

    public Vector2 MoveInput
    {
        get => _moveInput;
        set => _moveInput = value;
    }
    public float MoveX
    {
        get => _moveInput.x;
        set => _moveInput.x = value;
    }

    public float MoveY
    {
        get=> _moveInput.y;
        set=>   _moveInput.y = value;
    }

    public bool JumpPressed
    {
        get; 
        set;
    }

    public bool AttackPressed { get; set; }

    public bool JumpHeld
    {
        get;
        set;
    }

    public Vector2 AnimatorDelta { get; set; }

    private BoolReactiveProperty _isJumping = new BoolReactiveProperty(false);

    public BoolReactiveProperty IsJumpingProperty => _isJumping ??= new BoolReactiveProperty(false);
    
    public bool IsJumping
    {
        get => IsJumpingProperty.Value;
        set => IsJumpingProperty.Value = value;
    }

    /// <summary>
    /// is the player currently trying to drop through 1-way platforms 
    /// </summary>
    public bool IsDropping
    {
        get;
        set;
    }

    
    public bool IsDead
    {
        get; 
        set;
    }

    
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

    #region [Private variables]

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

    /// <summary>
    /// extends the duration that the player is allowed to 
    /// </summary>
    public float ExtraJumpTime { get; set; }

    /// <summary>
    /// increases the character's jump force (allowing them to rise more quickly and jump higher)
    /// </summary>
    public float ExtraJumpForce
    {
        get => extraJumpForce;
        set => extraJumpForce = value;
    }

    [NonSerialized,SerializeField] private float extraJumpForce = 0;
     
    public float ExtraJumpForceConsumed
    {
        get => extraJumpConsumedValue.ConsumeValue();
        set => extraJumpConsumedValue.Amount = value;
    }


    [SerializeField]
    internal ConsumedValue extraJumpConsumedValue;

    [Serializable]
    public class ConsumedValue
    {
        [SerializeField] private float amount = 10;
        public float consumptionRate = 4;
        public float consumptionRateAccel = 1;
        
        public float Amount
        {
            set => amount = value;
        }

        private float _currentRate;
        private float _lastConsumptionTime;
        public float ConsumeValue()
        {
            float maxRate = consumptionRate;
            float delta = Time.time - _lastConsumptionTime;
            
            _currentRate = 
                consumptionRateAccel > 0 
                ? Mathf.MoveTowards(_currentRate, maxRate, delta * consumptionRateAccel) 
                : consumptionRate;
            
            _lastConsumptionTime = Time.time;
            var amountToConsume = Mathf.Min(_currentRate, amount);
            amount -= amountToConsume;
            return amountToConsume;
        }
    }
    
    
    
    internal Subject<Unit> onLanded = new Subject<Unit>();
    public IObservable<Unit> OnCharacterLanded => onJumped;
    
    internal Subject<Unit> onJumped = new Subject<Unit>();
    public IObservable<Unit> OnCharacterJumped => onJumped;
}

public enum InteractionPhysicsMode
{
    Default = 0,  //default, root motion controls character's movement
    Mixed = 1,      //root motion is added with gravity
    FullPhysics = 2 //root motion is applied as a force
}