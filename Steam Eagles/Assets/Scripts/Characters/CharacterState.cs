using System;
using System.Collections.Generic;
using DefaultNamespace;
using UniRx;
using UnityEngine;

[RequireComponent(typeof(GroundCheck))]
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterState : MonoBehaviour
{

    public CharacterConfig config;
    private BoolReactiveProperty _isJumping = new BoolReactiveProperty(false);
    public bool alwaysGrounded = false;
    private readonly BoolReactiveProperty _isGroundedProperty = new BoolReactiveProperty(false);
    private readonly BoolReactiveProperty _isInteractingProperty = new BoolReactiveProperty(false);
    private Vector2 _moveInput;

    
    #region Public variables
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
    
    


    public bool IsGrounded
    {
        get => _isGroundedProperty.Value || alwaysGrounded;
        set => _isGroundedProperty.Value = value;
    }

    /// <summary>
    /// wrapper for x component of Rigidbody2D.velocity
    /// </summary>
    public float VelocityX
    {
        get => Rigidbody.velocity.x;
        set => Rigidbody.velocity = new Vector2(value, Rigidbody.velocity.y);
    }

    /// <summary>
    /// wrapper for y component of Rigidbody2D.velocity
    /// </summary>
    public float VelocityY
    {
        get => Rigidbody.velocity.y;
        set => Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, value);
    }

    /// <summary>
    /// wrapper for Rigidbody2D.velocity
    /// </summary>
    public Vector2 Velocity
    {
        get => Rigidbody.velocity;
        set => Rigidbody.velocity = value;
    }

    public float LiftForce
    {
        get;
        set;
    }

    
    [System.Obsolete("Unsure if this is needed")]
    public List<Vector4> Forces { get; set; }
    
    
    public Vector2 AnimatorAccel { get; set; }
    public bool StunLocked { get; set; }


    #region [RxStreams]

    public BoolReactiveProperty IsJumpingProperty => _isJumping ??= new BoolReactiveProperty(false);
    public IObservable<bool> IsInteractingStream => _isInteractingProperty;
    public IObservable<bool> IsGroundedEventStream => !alwaysGrounded ? _isGroundedProperty : Observable.Return(true);

    #endregion
    
    #endregion

    #region [Private variables]

    
   

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

    #region [Consumed Value Experiment]

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

    #endregion

    #region [RxStreams]


    internal Subject<Unit> onLanded = new Subject<Unit>();
    public IObservable<Unit> OnCharacterLanded => onJumped;
    
    internal Subject<Unit> onJumped = new Subject<Unit>();
    public IObservable<Unit> OnCharacterJumped => onJumped;
    
    internal ReactiveProperty<Rigidbody2D> heldObject = new ReactiveProperty<Rigidbody2D>();
    public IObservable<Rigidbody2D> HeldObject => heldObject;

    #endregion



    public Rigidbody2D toAttach;
    private FixedJoint2D _fixedJoint;

 
    
    
    private Rigidbody2D _attachedBody;
    public Rigidbody2D AttachedBody
    {
        get => _attachedBody;
        set
        {
            if (value != _attachedBody)
            {
                if (_attachedBody != null)
                {
                    Destroy(_fixedJoint);
                }
                _attachedBody = value;
                if (_attachedBody != null)
                {     
                    _fixedJoint = _attachedBody.gameObject.AddComponent<FixedJoint2D>();
                    _fixedJoint.connectedBody = this.Rigidbody;
                }
            }
        }
    }

    public bool CheckAttached()
    {
        if(toAttach != null)
        {
            AttachedBody = toAttach;
            toAttach = null;
            return true;
        }
        return AttachedBody != null;
    }
}

public enum InteractionPhysicsMode
{
    Default = 0,  //default, root motion controls character's movement
    Mixed = 1,      //root motion is added with gravity
    FullPhysics = 2 //root motion is applied as a force
}