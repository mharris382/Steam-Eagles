using System;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace AI.Enemies
{
    public interface IEnemyController
    {
        bool IsGrounded { get; }
        bool IsFacingRight { get; }
        float VelocityY { get; }
        
        IObservable<Unit> OnAttack();
        IObservable<Unit> OnJump();
        IObservable<Unit> OnDamaged();

        void DoJumpTakeoff();
        void DoAttack();
        
        void SetStunned(bool value);
    }

    public class EnemyAnimationHelper : MonoBehaviour
    {
        private Animator _anim;
        private IEnemyController _controller;
        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int VelocityY = Animator.StringToHash("Velocity Y");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        private static readonly int IsFacingRightHash = Animator.StringToHash("Is Facing Right");
        private static readonly int MovementLockedHash = Animator.StringToHash("Movement Locked");

        public bool invertDirection;
        private BoolReactiveProperty _isFacingRight = new();


        public bool MovementLocked => _anim.GetBool(MovementLockedHash);

        public bool IsFacingRight
        {
            get => _isFacingRight.Value;
            set => SetFacingRight(value);
        }
        
        public void Set(IEnemyController controller)
        {
            this._controller = controller;
            _anim = GetComponent<Animator>();
            _controller.OnAttack().Subscribe(_ => _anim.SetTrigger(Attack)).AddTo(this);
            _controller.OnJump().Subscribe(_ => _anim.SetTrigger(Jump)).AddTo(this);
        }

        bool HasResources() => _controller != null;

        private void Awake()
        {
            _anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!HasResources()) return;
            _anim.SetFloat(VelocityY, _controller.VelocityY);
            _anim.SetBool(Grounded, _controller.IsGrounded);
        }

        public void DoJumpTakeoff()
        {
            if (!HasResources()) return;
            _controller.DoJumpTakeoff();
        }

        public void DoAttack()
        {
            if (!HasResources()) return;
            _controller.DoAttack();
        }
        
        public void SetFacingRight(bool facingRight)
        {
            _isFacingRight.Value = facingRight;
            _anim.SetBool(IsFacingRightHash, facingRight);
        }
    }
}