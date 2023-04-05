﻿using System;
using CoreLib;
using UnityEngine;
using FSM;
using Sirenix.OdinInspector;
using Spine.Unity;
using UnityEngine.Events;
using Fsm = FSM.StateMachine;
namespace Characters.Animations
{
    [InfoBox("State Names are:\n Idle, Run, Jump, Fall")]
    [RequireComponent(typeof(SpineAnimationHandler))]
    public class CharacterAnimatorController : MonoBehaviour
    {
        [SerializeField,Required] private Transform aimTarget;
        [SerializeField] private UnityEvent<string> onStateChange;
        [SpineAnimation()] public string idleAnimationName;
        [SpineAnimation()] public string runAnimationName;
        [SpineAnimation()] public string jumpAnimationName;

        public bool debug = true;

        private SpineAnimationHandler _animationHandler;
        private CharacterState _characterState;
        private ToolState _characterToolState;
        private SkeletonAnimation _skeletonAnimation;
        private SkinController _skinController;
        private Fsm _stateMachine;

        void Awake()
        {
            _characterState = GetComponentInParent<CharacterState>();
            _characterToolState = _characterState.Tool;
            _skeletonAnimation = GetComponent<SkeletonAnimation>();
            _animationHandler = GetComponent<SpineAnimationHandler>();
            _skinController = GetComponent<SkinController>();
        }

        private void Start()
        {
            _stateMachine = new Fsm();
            var stateMachine = CreateDefaultFSM();
            var toolFsm = CreateToolFSM();
           
            _stateMachine.AddState("Default", stateMachine);
            _stateMachine.AddState("Tool", toolFsm);
            
            _stateMachine.AddTransition("Default", "Tool" , _ => _characterState.UsingTool);
            _stateMachine.AddTransition("Tool" , "Default", _ => !_characterState.UsingTool);
            
            _stateMachine.SetStartState("Default");
            _stateMachine.Init();
        }

        #region [Create State Machines]

        Fsm CreateToolFSM()
        {
            var toolFsm = new Fsm();
            
            Debug.Assert(_skeletonAnimation.skeletonDataAsset != null, $"Spine Animation Controller with skeleton({_skeletonAnimation.skeletonDataAsset.name}) has no skeleton data asset specified, add one in prefab ({_characterState.name})!", this);
            Debug.Assert(aimTarget != null, $"Spine Animation Controller with skeleton({_skeletonAnimation.skeletonDataAsset.name}) has no aim target transform specified, add one in prefab ({_characterState.name})!", this);
            
            AddToolStateFromStateObject(ToolStates.Repair, 
                new ToolStateRepairTool(_characterState, _characterToolState, _skeletonAnimation, _skinController, aimTarget, false));
            
            AddToolStateFromStateObject(ToolStates.Build,
                new ToolStateBuildTool(_characterState, _characterToolState, _skeletonAnimation, _skinController, aimTarget, false));
            
            AddToolStateFromStateObject(ToolStates.Destruct,
                new ToolStateDestructTool(_characterState, _characterToolState, _skeletonAnimation, _skinController, aimTarget, false));
            
            AddToolStateFromStateObject(ToolStates.Recipe, 
                new ToolStateRecipeBookTool(_characterState, _characterToolState, _skeletonAnimation, _skinController, aimTarget, false));
            
            toolFsm.Init();
            return toolFsm;


            void AddToolStateFromStateObject(ToolStates toolStates, StateBase stateBase)
            {
                var toolState = toolStates.ToString();
                toolFsm.AddState(toolState, stateBase);
                toolFsm.AddTransitionFromAny(toolState, t => _characterToolState.currentToolState == toolStates);
            }

            void AddToolState(ToolStates state)
            {
                var toolState = state.ToString();
                toolFsm.AddState(toolState
                    ,
                    onEnter: _ => { Debug.Log($"Animator Enter Logic for {toolState.Bolded()}", this); },
                    onLogic: _ => { Debug.Log($"Animator Update Logic for {toolState.Bolded()}", this); },
                    onExit: _ => { Debug.Log($"Animator Exit Logic for {toolState.Bolded()}", this); });
                toolFsm.AddTransitionFromAny(toolState, t => _characterToolState.currentToolState == state);
            }
        }

        Fsm CreateDefaultFSM()
        {
            var stateMachine = new Fsm();
            var groundedFsm = CreateGroundedFSM();
            var aerialFsm = CreateAerialFSM();
            
            //---------------------------
            
            stateMachine.AddState("Air", aerialFsm);
            stateMachine.AddState("Grounded", groundedFsm);
            
            stateMachine.AddTransition("Grounded", "Air", t => CheckAirCondition());
            stateMachine.AddTransition("Air", "Grounded", t => IsGrounded());
            
            stateMachine.SetStartState("Grounded");
            stateMachine.Init();
            return stateMachine;
        }

        Fsm CreateGroundedFSM()
        {
            var groundedFsm = new Fsm(needsExitTime: false);
            groundedFsm.AddState(
                "Idle",
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Idle", 0);
                    _skinController.UpdateState("Idle");
                    LogState("Idle");
                },
                onLogic: state => { UpdateFacingDirection(); });

            groundedFsm.AddState(
                "Run",
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Run", 0);
                    _skinController.UpdateState("Run");
                    LogState("Run");
                },
                onLogic: state => { UpdateFacingDirection(); });

            groundedFsm.AddTransition("Idle", "Run", t => IsMoving());
            groundedFsm.AddTransition("Run", "Idle", t => !IsMoving());
            groundedFsm.SetStartState("Idle");
            groundedFsm.Init();
            return groundedFsm;
        }

        Fsm CreateAerialFSM()
        {
            var aerialFsm = new Fsm();
            
            aerialFsm.AddState(
                "Jump", 
                onEnter: state =>
                {
                    _animationHandler.PlayAnimationForState("Jump", 0);
                    _skinController.UpdateState("Jump");
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
                    _skinController.UpdateState("Fall");
                    LogState("Fall");
                },
                onLogic: state =>
                {
                    UpdateFacingDirection();
                });

            aerialFsm.AddTransition("Jump", "Fall", t => !_characterState.IsJumping);
            aerialFsm.AddTransition("Falling", "Jump", t => _characterState.IsJumping);
            aerialFsm.SetStartState("Fall");
            aerialFsm.Init();
            return aerialFsm;
        }

        #endregion

        private void Update()
        {
            _stateMachine.OnLogic();
        }


        public void UpdateFacingDirection()
        {
            _skeletonAnimation.skeleton.ScaleX = _characterState.FacingRight ? 1 : -1;
        }
        
        

        bool CheckAirCondition()
        {
            if (_characterState.IsJumping)
                return true;
            return !_characterState.IsGrounded;
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