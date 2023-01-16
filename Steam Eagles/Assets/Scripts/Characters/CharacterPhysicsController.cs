using System;
using DefaultNamespace;
using UniRx;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// replacement for CharacterController which is huge and buggy.
    /// <see cref="https://www.youtube.com/watch?v=QPiZSTEuZnw&t=1308s&ab_channel=Bardent"/>
    /// 
    /// </summary>
    [RequireComponent(typeof(IGroundCheck))]
    [RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(CharacterInputState), typeof(CharacterState))]
    public class CharacterPhysicsController : MonoBehaviour
    {
        [SerializeField] private float groundCheckRadius = .25f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private LayerMask groundMask;
        
        private CapsuleCollider2D _capsuleCollider;
        private Rigidbody2D _rigidbody;
        private CharacterInputState _inputState;
        private CharacterState _characterState;
        private IGroundCheck _groundCheck;

        
        private PhysicsMaterial2D noFrictionMaterial;
        private PhysicsMaterial2D fullFrictionMaterial;
        public float movementSpeed => _characterState.config.moveSpeed;
        public float jumpForce => _characterState.config.jumpForce;
        private CharacterConfig config => _characterState.config;
        
        private Vector2 capsuleColliderSize;
        private ReadOnlyReactiveProperty<bool> readRX;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            _inputState = GetComponent<CharacterInputState>();
            _characterState = GetComponent<CharacterState>();
            _groundCheck = GetComponent<IGroundCheck>();
        }
        
        private void Start()
        {
            groundMask = config.GetGroundLayers();
            fullFrictionMaterial = config.GetFullFrictionMaterial();
            noFrictionMaterial = config.GetNoFrictionMaterial();
            capsuleColliderSize = _capsuleCollider.size;
            _groundCheck.OnGroundedStateChanged += OnGroundedChanged;

            this.readRX = new ReadOnlyReactiveProperty<bool>(Observable.FromEvent<bool>(
                t => _groundCheck.OnGroundedStateChanged += t, t => _groundCheck.OnGroundedStateChanged -= t));
            readRX.Subscribe(OnGroundedChanged).AddTo(this);
            _characterState.ObserveEveryValueChanged(t => t.MoveInput.x)
                .Select(t => Mathf.Abs(t )< 0.1f)
                .Subscribe(OnGroundedChanged).AddTo(this);
        }

        private void OnGroundedChanged(bool isGrounded)
        {
            _capsuleCollider.sharedMaterial = (isGrounded && readRX.Value) ? fullFrictionMaterial : noFrictionMaterial;
        }
    }


}