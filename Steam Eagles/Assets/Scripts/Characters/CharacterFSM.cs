using System;
using FSM;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class CharacterFSM : MonoBehaviour
    {
        [Range(0.001f, 1), SerializeField] private float dropTime = 0.25f;
        [SerializeField] private float dropSpeed = 5f;
        [SerializeField] private bool debug = true;
        
        private CharacterInputState _input;
        private CharacterController2 _controller;
        
        private FSM.StateMachine _physicsStateMachine;
        private CharacterState _state;

        public CharacterInputState Input => _input;
        public CharacterController2 Controller => _controller;
        public CharacterState State => _state;

        private bool _jumped;
        private float _jumpTime;
        
        private const string DEFAULT = "Default";
        private const string DROPPING = "Dropping";
        private const string JUMPING = "Jumping";

        private void Awake()
        {
            _controller = GetComponent<CharacterController2>();
            _physicsStateMachine = new FSM.StateMachine();
            _state = GetComponent<CharacterState>();
            _input = GetComponent<CharacterInputState>();

            _input.onJump.AsObservable().Subscribe(t =>
            {
                
                switch (t.phase)
                {
                    case InputActionPhase.Disabled:
                        break;
                    case InputActionPhase.Waiting:
                        break;
                    case InputActionPhase.Started:
                        Debug.Log("Jump Started");
                        break;
                    case InputActionPhase.Performed:
                        Debug.Log("Jump Performed");
                        break;
                    case InputActionPhase.Canceled:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            });
            
            
           

            #region DROP METHODS

            void OnPlatformDropEnter(State<string, string> t)
            {
                LogStateEntered("Platform Drop");
                Controller.SetPhysicsMaterial(Controller.FrictionlessPhysicsMaterial);
                Controller.BeginDropping();
                Controller.ClearParent();
                State.IsDropping = true;
            }

            void OnPlatformDropExit(State<string, string> t)
            {
                Controller.UpdatePhysMat();
                Controller.StopDropping();
                State.IsDropping = false;
            }

            void OnPlatformDropLogic(State<string, string> t)
            {
                if (t.timer.Elapsed > dropTime)
                {
                    t.fsm.StateCanExit();
                    t.fsm.RequestStateChange(DEFAULT);
                }
                else
                {
                    Controller.rb.velocity.Set(Controller.rb.velocity.x, -dropSpeed);
                    Controller.ApplyHorizontalMovement();
                }
            }

            #endregion

            #region JUMP METHODS

            void OnJumpEnter(State<string, string> t)
            {
                Controller.BeginJump();
                _jumpTime = Time.time;
                if (Controller.IsBalloonJumping && Controller.BalloonCollider != null &&
                    Controller.BalloonCollider.attachedRigidbody != null)
                {
                    var force = Vector2.down * Controller.Config.balloonImpactForce * Controller.rb.mass;
                    Controller.BalloonCollider.attachedRigidbody.AddForceAtPosition(Controller.groundCheck.position, force, ForceMode2D.Impulse);
                }
            }

            void OnJumpLogic(State<string, string> t)
            {
                Controller.UpdateFacingDirection();
                Controller.ApplyHorizontalMovement();
                Controller.ApplyJumpForce();
                Controller.ClearParent();
            }
            
            void OnJumpExit(State<string, string> t)
            {
                Controller.EndJump();
            }

            #endregion
            
            _physicsStateMachine.AddState(DEFAULT, onLogic: t =>
            {
                Controller.UpdateFacingDirection();
                Controller.UpdateGround();
                Controller.CheckWater();
                Controller.UpdateSlopes();
                Controller.UpdatePhysMat();
                Controller.ApplyMovement();
                Controller.CheckParent();
            });
            
            _physicsStateMachine.AddState(
                DROPPING,
                onEnter: OnPlatformDropEnter,
                onLogic: OnPlatformDropLogic,
                onExit: OnPlatformDropExit, 
                needsExitTime:true);
            
            _physicsStateMachine.AddState(
                JUMPING,
                onEnter: OnJumpEnter,
                onLogic: OnJumpLogic, 
                onExit: OnJumpExit, 
                needsExitTime:false);

            _physicsStateMachine.AddTransition(DEFAULT, JUMPING, t => CheckJumpCondition());
            _physicsStateMachine.AddTransition(JUMPING, DEFAULT, t => !State.IsJumping || !State.JumpHeld);

            _physicsStateMachine.AddTransition(DEFAULT, DROPPING, (t) => CheckDropCondition());
            _physicsStateMachine.AddTransition(DROPPING, DEFAULT);

            _physicsStateMachine.SetStartState(DEFAULT);
            _physicsStateMachine.Init();
        }

       


        public bool CheckJumpCondition() => Controller.AbleToJump() && (State.JumpPressed || _state.JumpHeld);

        public bool CheckDropCondition() => Controller.AbleToDrop() && State.DropPressed;

        private void Update()
        {
            Controller.UpdateJumpTimer(Time.deltaTime);
            Controller.CheckJumping();
        }

        private void FixedUpdate()
        {
            _physicsStateMachine.OnLogic();
        }


        private void LogStateEntered(string stateName)
        {
            if (debug)
            {
                Debug.Log($"{name} Entered State: {stateName}", this);
            }
        }
    }
}