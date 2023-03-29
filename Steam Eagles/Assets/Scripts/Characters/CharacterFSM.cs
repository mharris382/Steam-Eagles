using System;
using System.Security.Cryptography;
using CoreLib;
using CoreLib.Interactions;
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
        
        
        private FSM.StateMachine _defaultStateMachine;
        private FSM.StateMachine _toolStateMachine;
        private CharacterState _state;
        private StructureState _structureState;
        private IPilot _pilot;
        public CharacterInputState Input => _input;
        public CharacterController2 Controller => _controller;
        public CharacterState State => _state;
        
        public StructureState StructureState => _structureState != null ? _structureState : _structureState = GetComponent<StructureState>();
        

        private bool _jumped;
        private float _jumpTime;
        
        private const string DEFAULT = "Default";
        private const string DROPPING = "Dropping";
        private const string JUMPING = "Jumping";
        private const string CLIMBING = "Climbing";
        private const string GROUNDED = "Grounded";
        private const string AERIAL = "Aerial";

        /// <summary>
        /// null object used since haven't yet implemented the pilot interface on character side
        /// </summary>
        private class NullPilot : IPilot
        {
            public string tag => owner.tag;
            private CharacterState _characterState;
            public NullPilot(GameObject owner)
            {
                this.owner = owner;
                _characterState = this.owner.GetComponent<CharacterState>();
            }

            public float XInput => _characterState.MoveX;
            public float YInput => _characterState.MoveY;
            public event Action<int> OnPowerToThrustersChanged;
            public event Action<int> OnPowerToHeatChanged;
            private GameObject owner;
            public void NotifyGainControls(AirshipControls controls)
            {
                Debug.Log($"Null Pilot gained controls of {controls.name}");
            }

            public void NotifyLostControls(AirshipControls controls)
            {
                Debug.Log($"Null Pilot lost control of {controls.name}");
            }
        }

        private NullPilot _nullPilot;
        
        private IPilot Pilot => _pilot ?? _nullPilot;

        private void Awake()
        {
            _nullPilot = new NullPilot(gameObject);
            _pilot = GetComponent<IPilot>();
            if (_pilot == null)
                Debug.LogWarning($"No Pilot implementor found on {name}. Using NullPilot instead.", this);

            MessageBroker.Default.Receive<AirshipPilotChangedInfo>()
                .Subscribe(pilotChanged =>
                {
                    if (CompareTag(pilotChanged.newPilotName))
                    {
                        pilotChanged.controls.AssignPilot(Pilot);
                        Debug.Log($"Assigned {tag} as pilot for {pilotChanged.controls.name}", this);
                    }
                    else if (pilotChanged.controls.CurrentPilot == Pilot)
                    {
                        pilotChanged.controls.AssignPilot(null);
                        Debug.Log($"Unassigned {tag} as pilot for {pilotChanged.controls.name}", this);
                    }
                })
                .AddTo(this);
            
            MessageBroker.Default.Receive<AirshipPilotChangedInfo>()
                .Select(t => !string.IsNullOrEmpty(t.newPilotName) && CompareTag(t.newPilotName))
                .Subscribe(isPilot => State.IsPilot = isPilot)
                .AddTo(this);

        }

        private void Start()
        {
            _controller = GetComponent<CharacterController2>();
            _state = GetComponent<CharacterState>();
            _input = GetComponent<CharacterInputState>();
            _input.onJump.AsObservable().Subscribe(t => {
                
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
                    Controller.ApplyHorizontalMovement(Time.fixedDeltaTime);
                }
            }

            #endregion

            #region JUMP METHODS

            void OnJumpEnter(State<string, string> t)
            {
                Controller.BeginJump(); 
                StructureState.Mode = StructureState.JointMode.DISABLED;
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
                Controller.ApplyHorizontalMovement(Time.fixedDeltaTime);
                Controller.ApplyJumpForce();
                Controller.ClearParent();
            }
            
            void OnJumpExit(State<string, string> t)
            {
                Controller.EndJump();
            }

            #endregion

            _defaultStateMachine = new FSM.StateMachine();
            var physicsFSM = new FSM.StateMachine();
            var standardPhyiscsFSM = new FSM.StateMachine();
            
            
            
            
            // physicsFSM.AddState(DEFAULT,
            //     onEnter: t =>
            //     {
            //         StructureState.Mode = StructureState.JointMode.AUTOMATIC;  
            //     },
            //     onLogic: t =>
            //     {
            //         
            //         Controller.UpdateFacingDirection();
            //         Controller.UpdateGround();
            //         Controller.CheckWater();
            //         Controller.UpdateSlopes();
            //         Controller.UpdatePhysMat();
            //         Controller.ApplyMovement(Time.fixedDeltaTime);
            //         Controller.CheckParent();
            //     });

            #region [Setting UP Standard Physics FSM]

            standardPhyiscsFSM.AddState(AERIAL, 
                onEnter: t =>
                {
                    StructureState.Mode = StructureState.JointMode.DISABLED;
                    Controller.UpdatePhysMat();
                },
                onLogic: t =>
                {
                    Controller.UpdateFacingDirection();
                    Controller.UpdateGround();
                    Controller.CheckWater();
                    Controller.ApplyHorizontalMovement(Time.fixedDeltaTime);
                    Controller.CheckParent();
                });
            
            standardPhyiscsFSM.AddState(GROUNDED, 
                onEnter: t =>
                {
                    StructureState.CheckForStructures();
                    StructureState.Mode = StructureState.JointMode.ENABLED;
                },
                onLogic: t =>
                {
                    StructureState.CheckForStructures();
                    Controller.UpdateFacingDirection();
                    Controller.UpdateGround();
                    Controller.CheckWater();
                    Controller.UpdateSlopes();
                    Controller.UpdatePhysMat();
                    Controller.ApplyMovement(Time.fixedDeltaTime);
                    Controller.CheckParent();
                });
            
            standardPhyiscsFSM.SetStartState(AERIAL);
            standardPhyiscsFSM.AddTransition(AERIAL, GROUNDED, _ => State.IsGrounded);
            standardPhyiscsFSM.AddTransition(GROUNDED, AERIAL, _ => !State.IsGrounded);
            standardPhyiscsFSM.Init();
            
            physicsFSM.AddState(DEFAULT, standardPhyiscsFSM);
            physicsFSM.AddState(CLIMBING,
                onEnter: t =>
                {
                    StructureState.Mode = StructureState.JointMode.ENABLED;
                },
                onLogic: t =>
                {
                    StructureState.CheckForStructures();
                    Controller.UpdateFacingDirection();
                    Controller.UpdateGround();
                    Controller.CheckParent();
                });
            
            physicsFSM.AddState(
                DROPPING,
                onEnter: OnPlatformDropEnter,
                onLogic: OnPlatformDropLogic,
                onExit: OnPlatformDropExit, 
                needsExitTime:true);
            
            physicsFSM.AddState(
                JUMPING,
                onEnter: OnJumpEnter,
                onLogic: OnJumpLogic, 
                onExit: OnJumpExit, 
                needsExitTime:false);

            physicsFSM.AddTransition(DEFAULT, JUMPING, _ => CheckJumpCondition());
            physicsFSM.AddTransition(JUMPING, DEFAULT, _ => !State.IsJumping || !State.JumpHeld);
            
            physicsFSM.AddTransition(DEFAULT, DROPPING, _ => CheckDropCondition() );
            physicsFSM.AddTransition(DROPPING, DEFAULT);
            
            physicsFSM.AddTransition(CLIMBING, DEFAULT, _ => !State.IsClimbing && !State.IsJumping);
            physicsFSM.AddTransition(CLIMBING, JUMPING, _ => State.IsJumping);
            physicsFSM.AddTransitionFromAny(CLIMBING, _ => CheckClimbStartCondition());

            physicsFSM.SetStartState(DEFAULT);
            physicsFSM.Init();

            #endregion
            
            _defaultStateMachine.AddState("Default", physicsFSM);
            _defaultStateMachine.AddState("Pilot", 
                onEnter: _ =>
                {
                    StructureState.Mode = StructureState.JointMode.ENABLED;//strap in
                    Debug.Log($"{tag} entered Pilot Mode",this);
                }, onLogic: _ =>
                {
                    Debug.Log($"{tag} Piloting",this);
                }, onExit: _ =>
                {
                    Debug.Log($"{tag} exited Pilot Mode",this);
                });
            _defaultStateMachine.AddTransition("Default", "Pilot", _ => State.IsPilot);
            _defaultStateMachine.AddTransition("Pilot", "Default", _ => !State.IsPilot);
            _defaultStateMachine.SetStartState("Default");
            _defaultStateMachine.Init();
        }

       

        public bool CheckClimbStartCondition()
        {
            return State.IsClimbing;
        }

        public bool CheckJumpCondition() => Controller.AbleToJump() && (State.JumpPressed || _state.JumpHeld);

        public bool CheckDropCondition() => (Controller.AbleToDrop() && State.DropPressed);

        private void Update()
        {
            Controller.UpdateJumpTimer(Time.deltaTime);
            Controller.CheckJumping();
        }

        private void FixedUpdate()
        {
            _defaultStateMachine.OnLogic();
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