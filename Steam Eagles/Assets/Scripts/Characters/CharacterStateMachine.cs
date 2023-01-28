using System;
using UnityEngine;
using FSMStateMachine = FSM.StateMachine;
using FSM;
namespace Characters
{
    [RequireComponent(typeof(CharacterState))]
    public class CharacterStateMachine : MonoBehaviour
    {
        internal enum UpdateLoop {
            NONE,
            UPDATE,
            LATE_UPDATE,
            FIXED_UPDATE,
        }

        private UpdateLoop _currentUpdateLoop = UpdateLoop.NONE;
        private CharacterState _state;

        private CharacterInputState _inputState;

        private FSMStateMachine _stateMachine;

        private Rigidbody2D _rigidbody2D;

        private CharacterState State => _state;

        private float MoveSpeed => State.config.moveSpeed;
        internal UpdateLoop CurrentUpdateLoop => _currentUpdateLoop;

        private void Awake()
        {
            _state = GetComponent<CharacterState>();
            _inputState = GetComponent<CharacterInputState>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            _stateMachine = new FSMStateMachine();
            _stateMachine.AddState("Jumping", onLogic: state => {
                if (CurrentUpdateLoop == UpdateLoop.UPDATE)
                {
                    
                }
            });
            _stateMachine.AddState("Aerial", onLogic: state => {
                    if(CurrentUpdateLoop == UpdateLoop.FIXED_UPDATE)
                        DoAerialMovement(Time.fixedDeltaTime);
                });

            var normalGameplayState = new FSMStateMachine();
            var groundedState = new FSMStateMachine();
            groundedState.AddState("SlopeMovement", onLogic: t =>
            {
                if (CurrentUpdateLoop == UpdateLoop.FIXED_UPDATE)
                {
                    
                }
            });
            groundedState.AddState("NormalMovement", onLogic: t =>
            {
                if (CurrentUpdateLoop == UpdateLoop.FIXED_UPDATE)
                {
                    
                }
            });
        }

        private void Update()
        {
            _currentUpdateLoop = UpdateLoop.UPDATE;
            _stateMachine.OnLogic();
        }

        private void FixedUpdate()
        {
            _currentUpdateLoop = UpdateLoop.FIXED_UPDATE;
            _stateMachine.OnLogic();
        }

        private void DoAerialMovement(float dt)
        {
            State.VelocityX = State.MoveX * MoveSpeed;
        }
        private bool CharacterIsValid()
        {
            return _state != null && _inputState != null && _inputState.IsAssigned();
        }
    }
}