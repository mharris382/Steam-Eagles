using System;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector.Editor.Modules;
using Spine.Unity;
using Fsm = FSM.StateMachine;
namespace Characters.Animations
{
    public class CharacterAnimatorController : MonoBehaviour
    {
        
        [SpineAnimation()] public string idleAnimationName;
        [SpineAnimation()] public string runAnimationName;
        [SpineAnimation()] public string jumpAnimationName;
        
        
        private CharacterState _characterState;
        private SkeletonAnimation _skeletonAnimation;
        private Fsm _stateMachine;

        private void Awake()
        {
            _characterState = GetComponentInParent<CharacterState>();
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _stateMachine = new Fsm();
            var groundedFsm = new Fsm(needsExitTime:false);
            groundedFsm.AddState("Idle",
                onEnter: state =>
                {
                    _skeletonAnimation.AnimationName = idleAnimationName;
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            groundedFsm.AddState("Run",
                onEnter: state =>
                {
                    _skeletonAnimation.AnimationName = runAnimationName;
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            groundedFsm.AddTransition("Idle", "Run",t=> IsMoving());
            groundedFsm.AddTransition("Run", "Idle",t=> !IsMoving());
            groundedFsm.SetStartState("Idle");
            _stateMachine.AddState("Grounded", groundedFsm);
            
            
            _stateMachine.AddState("Air", 
                onEnter: t =>
                {
                    _skeletonAnimation.AnimationName = jumpAnimationName;
                },onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            _stateMachine.AddTransition("Grounded", "Air", t => !IsGrounded());
            _stateMachine.AddTransition("Air", "Grounded", t => IsGrounded());
            
            _stateMachine.SetStartState("Grounded");
            _stateMachine.Init();
        }

        private void Update()
        {
            _stateMachine.OnLogic();
        }


        public void UpdateFacingDirection()
        {
            _skeletonAnimation.skeleton.ScaleX = _characterState.FacingRight ? 1 : -1;
        }

        bool IsMoving()
        {
            return Mathf.Abs(_characterState.MoveX) > 0.1f;
        }

        bool IsGrounded()
        {
            return _characterState.IsOnSolidGround;
        }
    }
}