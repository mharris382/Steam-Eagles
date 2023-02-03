using System;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor.Modules;
using Spine.Unity;
using Fsm = FSM.StateMachine;
namespace Characters.Animations
{
    [InfoBox("State Names are:\n Idle, Run, Jump, Fall")]
    [RequireComponent(typeof(SpineAnimationHandler))]
    public class CharacterAnimatorController : MonoBehaviour
    {
        
        [SpineAnimation()] public string idleAnimationName;
        [SpineAnimation()] public string runAnimationName;
        [SpineAnimation()] public string jumpAnimationName;
        
        public bool debug = true;
        
        private SpineAnimationHandler _animationHandler;
        private CharacterState _characterState;
        private SkeletonAnimation _skeletonAnimation;
        private Fsm _stateMachine;
        
        private void Start()
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
                    LogState("Idle");
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
                   LogState("Run");
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
                    LogState("Jump");
                }, 
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            aerialFsm.AddState(
                "Fall",
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Fall", 0);
                    LogState("Fall");
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });
            
            aerialFsm.AddTransition("Jump", "Fall", t => !_characterState.IsJumping);
            aerialFsm.AddTransition("Falling", "Jump", t => _characterState.IsJumping);
            aerialFsm.SetStartState("Jump");
            
            //---------------------------
            
            _stateMachine.AddState("Air", aerialFsm);
            _stateMachine.AddState("Grounded", groundedFsm);
            
            _stateMachine.AddTransition("Grounded", "Air", t => CheckAirCondition());
            _stateMachine.AddTransition("Air", "Grounded", t => IsGrounded());
            
            _stateMachine.SetStartState("Grounded");
            _stateMachine.Init();
        }

        bool CheckAirCondition()
        {
            if (_characterState.IsJumping)
                return true;
            return !_characterState.IsGrounded;
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
            return _characterState.IsGrounded && !_characterState.IsJumping;
        }


        void LogState(string stateName)
        {
            if (debug)
            {
                Debug.Log($"Animator {name} Entered State: " + stateName);
            }
        }
    }
}