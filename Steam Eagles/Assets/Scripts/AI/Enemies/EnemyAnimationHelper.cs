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

        public bool invertDirection;
        private BoolReactiveProperty _isFacingRight = new();
        public UnityEvent<bool> setIsFacingRight;
        
        public void Set(IEnemyController controller)
        {
            this._controller = controller;
            _anim = GetComponent<Animator>();
            _controller.OnAttack().Subscribe(_ => _anim.SetTrigger(Attack)).AddTo(this);
            _controller.OnJump().Subscribe(_ => _anim.SetTrigger(Jump)).AddTo(this);
            _isFacingRight.Subscribe(t => setIsFacingRight?.Invoke(invertDirection ? !t : t)).AddTo(this);
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
            _isFacingRight.Value = _controller.IsFacingRight;
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
    }
}