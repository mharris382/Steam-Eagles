using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace AI.Enemies
{
    [Serializable]
    public class GroundCheckConfig
    {
        [Required]
        public Transform groundCheck;
        public float groundCheckDistance = 1;
        public LayerMask groundLayer;

        public bool debug;

        public bool CheckGrounded()
        {
            var hit = Physics2D.Raycast(groundCheck.position, -groundCheck.up, groundCheckDistance, groundLayer);
            if (debug)
            {
                Debug.DrawRay(groundCheck.position, Vector2.down * groundCheckDistance, hit ? Color.green : Color.red);
            }
            return hit;
        }
    }

    [Serializable]
    public class JumpConfig
    {
        public float jumpForce = 10;
    }

    [Serializable]
    public class MoveConfig
    {
        public float moveSpeed = 5;
        public float airControl = 0.5f;
    }

    [RequireComponent(typeof(Health))]
    public class SlimeController : MonoBehaviour, IEnemyController
    {
        
        public GroundCheckConfig groundCheckConfig;
        public JumpConfig jumpConfig;
        public MoveConfig moveConfig;


        private Subject<Unit> _attackStart = new();
        private Subject<Unit> _jumpStart = new();
        private Subject<Unit> _damaged = new();

        private Rigidbody2D _rb;
        private Health _health;
        private bool _forceNotGrounded;
        private bool _grounded;
        private EnemyAnimationHelper _animHelper;
        private Health Health => _health ? _health : _health = GetComponent<Health>();
        private Rigidbody2D Rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();

        public bool IsGrounded
        {
            get => _grounded && !_forceNotGrounded;
            set => _grounded = value;
        }
        public float VelocityY => Rb.velocity.y;

        public bool IsAlive => !Health.IsDead;

        public bool IsFacingRight
        {
            get;
            set;
        }

        public IObservable<Unit> OnAttack() => _attackStart;

        public IObservable<Unit> OnJump() => _jumpStart.Where(t => !IsGrounded);

        public IObservable<Unit> OnDamaged() =>
            Health.OnValueChanged.Where(t => t.prevValue < t.newValue).AsUnitObservable();


        private void Awake()
        {

            
            _animHelper = GetComponentInChildren<EnemyAnimationHelper>();
            _animHelper.Set(this);
        }

        private void Update()
        {
            IsGrounded = groundCheckConfig.CheckGrounded();
        }
        public void StartJump()
        {
            if (IsGrounded && IsAlive)
            {
                _jumpStart.OnNext(Unit.Default);
            }
        }

        public void StartAttack()
        {
            if (IsGrounded && IsAlive)
            {
                _attackStart.OnNext(Unit.Default);
            }
        }
        public void SetMovement(float direction)
        {
            var velocity = Rb.velocity;
            velocity.x = Mathf.Sign(direction) * moveConfig.moveSpeed * (IsGrounded ? 1 : moveConfig.airControl);
            Rb.velocity = velocity;
            IsFacingRight = direction > 0;
        }

        public void DoJumpTakeoff()
        {
            Rb.AddForce(((Vector2.up + new Vector2(Rb.velocity.x, 0)).normalized * jumpConfig.jumpForce), ForceMode2D.Impulse);
            _forceNotGrounded = true;
            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => _forceNotGrounded = false);
        }

        public void DoAttack()
        {
            throw new NotImplementedException();
        }

        public void SetStunned(bool value)
        {
            throw new NotImplementedException();
        }


       
    }
}