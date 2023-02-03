using System;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector.Editor.Modules;
using Spine.Unity;
using Fsm = FSM.StateMachine;
namespace Characters.Animations
{
    [RequireComponent(typeof(SpineAnimationHandler))]
    public class CharacterAnimatorController : MonoBehaviour
    {
        
        [SpineAnimation()] public string idleAnimationName;
        [SpineAnimation()] public string runAnimationName;
        [SpineAnimation()] public string jumpAnimationName;
        
        private SpineAnimationHandler _animationHandler;
        private CharacterState _characterState;
        private SkeletonAnimation _skeletonAnimation;
        private Fsm _stateMachine;
        public bool useAnimationHandler = true;
        private void Awake()
        {
            _characterState = GetComponentInParent<CharacterState>();
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _animationHandler = GetComponent<SpineAnimationHandler>();
            _stateMachine = new Fsm();
            
            //---------------------------
            //Grounded State Machine
            
            var groundedFsm = new Fsm(needsExitTime:false);
            
            groundedFsm.AddState(
                "Idle",
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Idle", 0);
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            groundedFsm.AddState(
                "Run",
                onEnter: state =>
                {
                   _animationHandler.PlayAnimationForState("Run", 0);
                },
                onLogic: state => 
                {
                    UpdateFacingDirection();
                });
            
            groundedFsm.AddTransition("Idle", "Run",t=> IsMoving());
            groundedFsm.AddTransition("Run", "Idle",t=> !IsMoving());
            groundedFsm.SetStartState("Idle");
            
            //---------------------------
            //Aerial State Machine

            var aerialFsm = new Fsm();
            
            aerialFsm.AddState(
                "Jump", 
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Jump", 0);
                }, 
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            aerialFsm.AddState(
                "Falling",
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Fall", 0);
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            aerialFsm.AddTransition("Jump", "Falling", t => !_characterState.IsJumping);
            aerialFsm.AddTransition("Falling", "Jump", t => _characterState.IsJumping);
            aerialFsm.SetStartState("Jump");
            
            //---------------------------
            
            _stateMachine.AddState("Air", aerialFsm);
            _stateMachine.AddState("Grounded", groundedFsm);
            
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