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
                Controller.ApplyJumpMovement();
                Controller.ApplyGravity();
            });
            
            _physicsStateMachine.AddState(
                "Dropping",
                onEnter: t =>
                {
                    Controller.SetPhysicsMaterial(Controller.FrictionlessPhysicsMaterial);
                    Physics2D.IgnoreCollision(State.Collider, Controller.OneWayCollider, true);
                },
                onLogic: t =>
                {
                    if (t.timer.Elapsed > dropTime)
                        t.fsm.StateCanExit();
                    else
                    {
                        Controller.rb.velocity.Set(Controller.rb.velocity.x, -dropSpeed);
                        Controller.ApplyHorizontalMovement();
                    }
                },
                onExit: t =>
                {
                    Controller.UpdatePhysMat();
                    Physics2D.IgnoreCollision(State.Collider, Controller.OneWayCollider, false);
                    State.IsDropping = false;
                }, 
                needsExitTime:true);
            _physicsStateMachine.AddTransition("Default", "Dropping", (t) => State.IsDropping);
            _physicsStateMachine.AddTransition("Dropping", "Default");
            _physicsStateMachine.SetStartState("Default");
            _physicsStateMachine.Init();
        }

        private bool CanDrop()
        {

            if (Controller.OneWayCollider == null) return false;
            if (!State.IsGrounded) return false;

            return true;
        }

        public bool CheckDropCondition()
        {
            return false;
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