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
            MessageBroker.Default.Receive<JumpActionEvent>().Where(t => t.transform == transform)
                .Subscribe(t =>
                {
                    //if (t.context.performed && Controller.AbleToJump())
                    //{
                    //    _state.IsJumping = true;
                    //    _jumpTime = Time.time;
                    //    _physicsStateMachine.Trigger("Jump");
                    //}
                }).AddTo(this);
            _physicsStateMachine.AddState("Default", onLogic: t =>
            {
                Controller.CheckFacingDirection();
                Controller.CheckGround();
                Controller.CheckWater();
                Controller.CheckSlopes();
                Controller.UpdatePhysMat();
                Controller.ApplyMovement();
                Controller.ApplyGravity();
            });
            
            _physicsStateMachine.AddState(
                "Dropping",
                onEnter: OnPlatformDropEnter,
                onLogic: OnPlatformDropLogic,
                onExit: OnPlatformDropExit, 
                needsExitTime:true);
            
            void OnPlatformDropEnter(State<string, string> t)
            {
                LogStateEntered("Platform Drop");
                Controller.SetPhysicsMaterial(Controller.FrictionlessPhysicsMaterial);
                Controller.BeginDropping();
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
                    t.fsm.RequestStateChange("Default");
                }
                else
                {
                    Controller.rb.velocity.Set(Controller.rb.velocity.x, -dropSpeed);
                    Controller.ApplyHorizontalMovement();
                }
            }
            
            
            _physicsStateMachine.AddState("Jumping",
                onEnter: t => {
                    Controller.BeginJump();
                    _jumpTime = Time.time;
                    if (Controller.IsBalloonJumping && Controller.BalloonCollider != null &&
                        Controller.BalloonCollider.attachedRigidbody != null)
                    {
                        var force = Vector2.down * Controller.Config.balloonImpactForce * Controller.rb.mass;
                        Controller.BalloonCollider.attachedRigidbody.AddForceAtPosition(Controller.groundCheck.position, force, ForceMode2D.Impulse);
                    }
                },
                onLogic: t => {
                    Controller.CheckFacingDirection();
                    Controller.ApplyHorizontalMovement();
                    Controller.ApplyJumpForce();
                }, 
                onExit: t =>
                {
                    Controller.EndJump();
                }, needsExitTime:false);
            _physicsStateMachine.AddTransition("Default", "Jumping", t => CheckJumpCondition());
            _physicsStateMachine.AddTransition("Jumping", "Default", t => !State.IsJumping || !State.JumpHeld);
            _physicsStateMachine.AddTransition("Default", "Dropping", (t) => CheckDropCondition());
            _physicsStateMachine.AddTransition("Dropping", "Default");
            _physicsStateMachine.SetStartState("Default");
            //_physicsStateMachine.AddTriggerTransition("Jump", "Default", "Jumping");
            _physicsStateMachine.Init();
        }

       


        public bool CheckJumpCondition()
        {
            return Controller.AbleToJump() && (State.JumpPressed || _state.JumpHeld);
        }
        public bool CheckDropCondition()
        {
            return Controller.AbleToDrop() && State.DropPressed;
        }

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