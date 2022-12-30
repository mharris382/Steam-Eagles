using System;
using DefaultNamespace;
using UnityEngine;

namespace Characters
{
    /// <summary>
    /// replacement for CharacterController which is huge and buggy.
    /// <see cref="https://www.youtube.com/watch?v=QPiZSTEuZnw&t=1308s&ab_channel=Bardent"/>
    /// 
    /// </summary>
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

        
        private PhysicsMaterial2D noFrictionMaterial;
        private PhysicsMaterial2D fullFrictionMaterial;
        public float movementSpeed => _characterState.config.moveSpeed;
        public float jumpForce => _characterState.config.jumpForce;
        private CharacterConfig config => _characterState.config;
        

        
        
        private Vector2 capsuleColliderSize;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _capsuleCollider = GetComponent<CapsuleCollider2D>();
            _inputState = GetComponent<CharacterInputState>();
            _characterState = GetComponent<CharacterState>();
            
        }

        private void Start()
        {
            groundMask = config.GetGroundLayers();
            fullFrictionMaterial = config.GetFullFrictionMaterial();
            noFrictionMaterial = config.GetNoFrictionMaterial();
            capsuleColliderSize = _capsuleCollider.size;
        }
    }


}