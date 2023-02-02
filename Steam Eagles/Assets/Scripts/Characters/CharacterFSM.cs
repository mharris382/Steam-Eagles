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
        
        private CharacterInputState _input;
        private CharacterController2 _controller;
        private FSM.StateMachine _physicsStateMachine;
        private CharacterState _state;

        public CharacterInputState Input => _input;
        public CharacterController2 Controller => _controller;
        public CharacterState State => _state;
        
         
        
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
            
            _physicsStateMachine.AddState("Jumping",
                onEnter: t => {
                    Controller.BeginJump();
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
                });
            _physicsStateMachine.AddTransition("Default", "Jumping", t => CheckJumpCondition());
            _physicsStateMachine.AddTransition("Jumping", "Default", t => !State.IsJumping || !State.JumpHeld);
            _physicsStateMachine.AddTransition("Default", "Dropping", (t) => CheckDropCondition());
            _physicsStateMachine.AddTransition("Dropping", "Default");
            _physicsStateMachine.SetStartState("Default");
            _physicsStateMachine.Init();
        }

        private void OnPlatformDropEnter(State<string, string> t)
        {
            Controller.SetPhysicsMaterial(Controller.FrictionlessPhysicsMaterial);
            Controller.BeginDropping();
            State.IsDropping = true;
        }

        private void OnPlatformDropExit(State<string, string> t)
        {
            Controller.UpdatePhysMat();
            Controller.StopDropping();
            State.IsDropping = false;
        }

        private void OnPlatformDropLogic(State<string, string> t)
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


        public bool CheckJumpCondition()
        {
            return Controller.AbleToJump() && (State.JumpHeld || State.JumpPressed);
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
    }
}