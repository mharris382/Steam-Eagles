using System;


using CoreLib;
using CoreLib.Interactions;
using FSM;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Characters
{
    public class CharacterFSM : MonoBehaviour
    {
        [Range(0.001f, 1), SerializeField] private float dropTime = 0.25f;
        [SerializeField] private float dropSpeed = 5f;
        [SerializeField] private bool debug = true;
        [SerializeField] private TilemapClimbableValues climbableValues;
        private CharacterInputState _input;
        private CharacterController2 _controller;
        
        
        private FSM.StateMachine _defaultStateMachine;
        private FSM.StateMachine _toolStateMachine;
        private CharacterState _state;
        private StructureState _structureState;
        private CharacterInteractionState _interactionState;
        private Health _health;
        
        private IPilot _pilot;
        public CharacterInteractionState InteractionState => _interactionState;
        public CharacterInputState Input => _input;
        public CharacterController2 Controller => _controller;
        public CharacterState State => _state;
        
        public StructureState StructureState => _structureState != null ? _structureState : _structureState = GetComponent<StructureState>();


        
        private bool _jumped;
        private float _jumpTime;
        private CharacterClimbCheck _climbCheck;
        private CharacterClimbingController _climbingController;
        
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
            public void NotifyGainControls(IAirshipControls controls)
            {
                Debug.Log($"Null Pilot gained controls of {controls.name}");
            }

            public void NotifyLostControls(IAirshipControls controls)
            {
                Debug.Log($"Null Pilot lost control of {controls.name}");
            }
        }

        private SpawnPoints _spawnPoints;
        private NullPilot _nullPilot;
        // private EntityRoomState _roomState;

        
        // public EntityRoomState RoomState => _roomState != null ? _roomState : _roomState = GetComponent<EntityRoomState>();
        
        private IPilot Pilot => _pilot ?? _nullPilot;

        private void Awake()
        {
            _controller = GetComponent<CharacterController2>();
            _state = GetComponent<CharacterState>();
            _input = GetComponent<CharacterInputState>();
            _health = GetComponent<Health>();
            _structureState = GetComponent<StructureState>();
            _interactionState = GetComponent<CharacterInteractionState>();
            // _roomState = GetComponent<EntityRoomState>();
            _nullPilot = new NullPilot(gameObject);
            _climbCheck = new CharacterClimbCheck(_state, 
                new TilemapClimbableFactory(_structureState.BuildingRigidbodyProperty, climbableValues), 
                new ColliderClimbableFactory());
            _climbingController = new CharacterClimbingController(_state, _controller.Config, _climbCheck);
            
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
        
     
        [Inject] void Inject(SpawnPoints spawnPoints)
        {
            _spawnPoints = spawnPoints;
        }
            
       

        private void Start()
        {
            _defaultStateMachine = new FSM.StateMachine();
            var physicsFSM = new FSM.StateMachine();
            var standardPhyiscsFSM = new FSM.StateMachine();
            
            

            #region [Setting UP Standard Physics FSM]

            standardPhyiscsFSM.AddState(AERIAL, OnAirEnter, OnAirLogic, OnAirExit);
            standardPhyiscsFSM.AddState(GROUNDED, OnGroundedEnter, OnGroundedLogic, OnGroundedExit);
            
            standardPhyiscsFSM.SetStartState(AERIAL);
            standardPhyiscsFSM.AddTransition(AERIAL, GROUNDED, _ => State.IsGrounded);
            standardPhyiscsFSM.AddTransition(GROUNDED, AERIAL, _ => !State.IsGrounded);
            standardPhyiscsFSM.Init();
            
            physicsFSM.AddState(DEFAULT, standardPhyiscsFSM);
            physicsFSM.AddState(CLIMBING, OnClimbEnter, OnClimbLogic, OnClimbExit);
            physicsFSM.AddState(DROPPING, OnPlatformDropEnter, OnPlatformDropLogic, OnPlatformDropExit, needsExitTime:true);
            physicsFSM.AddState(JUMPING, OnJumpEnter, OnJumpLogic,OnJumpExit, needsExitTime:false);

            physicsFSM.AddTransition(DEFAULT, JUMPING, _ => CheckJumpCondition());
            physicsFSM.AddTransition(JUMPING, DEFAULT, _ => !State.IsJumping || !State.JumpHeld);
            
            physicsFSM.AddTransition(DEFAULT, DROPPING, _ => CheckDropCondition() );
            physicsFSM.AddTransition(DROPPING, DEFAULT);
            
            physicsFSM.AddTransition(CLIMBING, DEFAULT, _ => CheckClimbStopCondition() && !State.IsJumping);
            physicsFSM.AddTransition(CLIMBING, JUMPING, _ => CheckClimbStopCondition() && State.IsJumping);
            physicsFSM.AddTransitionFromAny(CLIMBING, _ => CheckClimbStartCondition());

            physicsFSM.SetStartState(DEFAULT);
            physicsFSM.Init();

            #endregion
            
            _defaultStateMachine.AddState("Default", physicsFSM);
            _defaultStateMachine.AddState("Interacting", OnInteractEnter, OnInteractLogic, OnInteractExit);
            _defaultStateMachine.AddState("Pilot", OnPilotEnter, OnPilotLogic, OnPilotExit);
            _defaultStateMachine.AddState("Dead", new DeadState(_health, _spawnPoints));
            
            _defaultStateMachine.AddTransition("Default", "Pilot", _ => State.IsPilot);
            _defaultStateMachine.AddTransition("Pilot", "Default", _ => !State.IsPilot);
            
            _defaultStateMachine.AddTransition("Interacting", "Default", _ => !InteractionState.IsInteracting);
            _defaultStateMachine.AddTransition("Default", "Interacting", _ => InteractionState.IsInteracting);
            _defaultStateMachine.AddTransitionFromAny("Dead", _ => _health.IsDead);
            _defaultStateMachine.AddTransition("Dead", "Default", _ => !_health.IsDead);
            _defaultStateMachine.SetStartState("Default");
            _defaultStateMachine.Init();
        }

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

        #region [Transition Checks]


        public bool CheckClimbStartCondition()
        {
            if (_climbCheck.ShouldCharacterStartClimbing())
            {
                return true;
            }
            return State.IsClimbing;
        }

        public bool CheckClimbStopCondition()
        {
            return _climbingController.ShouldStopClimbing();
        }

        public bool CheckJumpCondition() => Controller.AbleToJump() && (State.JumpPressed || _state.JumpHeld);

        public bool CheckDropCondition()
        {
            if (State.DropPressed)
            {
                State.DropPressed = false;
                return Controller.AbleToDrop();
            }
            return false;
        }

        #endregion

        #region AERIAL METHODS

        void OnAirEnter(State<string, string> t)
        {
            StructureState.Mode = StructureState.JointMode.DISABLED;
            Controller.UpdatePhysMat();
        }

        void OnAirLogic(State<string, string> t)
        {
            Controller.UpdateFacingDirection();
            Controller.UpdateGround();
            Controller.CheckWater();
            Controller.ApplyHorizontalMovement(Time.fixedDeltaTime);
            Controller.CheckParent();
        }

        void OnAirExit(State<string, string> t)
        {
            
        }

        #endregion 
        
        #region GROUNDED METHODS

        void OnGroundedEnter(State<string, string> t)
        {
            StructureState.CheckForStructures();
            StructureState.Mode = StructureState.JointMode.ENABLED;
        }

        void OnGroundedLogic(State<string, string> t)
        {
            
            StructureState.CheckForStructures();
            Controller.UpdateFacingDirection();
            Controller.UpdateGround();
            Controller.CheckWater();
            Controller.UpdateSlopes();
            Controller.UpdatePhysMat();
            Controller.ApplyMovement(Time.fixedDeltaTime);
            Controller.CheckParent();
        }

        void OnGroundedExit(State<string, string> t)
        {
            
        }

        #endregion 

        #region CLIIMB METHODS
        void OnClimbEnter(State<string, string> t)
        {
            _climbingController.OnClimbStart();
        }
        
        void OnClimbLogic(State<string, string> t)
        {
            _climbingController.UpdateClimbing(Time.fixedDeltaTime);
        }

        void OnClimbExit(State<string, string> t)
        {
            _climbingController.OnClimbStopped();
        }

        #endregion
        
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

        #region INTERACT METHODS

        void OnInteractEnter(State<string, string> t)
        {
            StructureState.Mode = StructureState.JointMode.ENABLED;    
        }
        
        void OnInteractLogic(State<string, string> t)
        {
            
        }
        
        void OnInteractExit(State<string, string> t)
        {
            StructureState.Mode = StructureState.JointMode.DISABLED;
        }
        

        #endregion
        #region PILOT METHODS

        
        void OnPilotEnter(State<string, string> t)
        {
            StructureState.Mode = StructureState.JointMode.ENABLED;//strap in
            Debug.Log($"{tag} entered Pilot Mode",this);
        }

        void OnPilotLogic(State<string, string> t)
        {
           
            Debug.Log($"{tag} Piloting",this);
        }

        void OnPilotExit(State<string, string> t)
        {
            
            Debug.Log($"{tag} exited Pilot Mode",this);
        }

        #endregion


        class DeadState : State
        {
            private readonly Health _health;
            private readonly SpawnPoints _spawnPoints;
            private readonly float _respawnDelay;
            private float _timestamp;


            public DeadState(Health health, SpawnPoints spawnPoints, float respawnDelay = 2) : base(needsExitTime:false)
            {
                _health = health;
                _spawnPoints = spawnPoints;
                _respawnDelay = respawnDelay;
            }

            public override void OnEnter()
            {
                _timestamp = Time.time;
                base.OnEnter();
            }

            public override void OnLogic()
            {
                if (Time.time - _timestamp > _respawnDelay)
                {
                    HandleRespawn();
                }
                base.OnLogic();
            }

            private void HandleRespawn()
            {
                _health.Respawn();
                var point = _spawnPoints.GetRespawnPoint(_health.transform.position);
                Debug.Assert(point != null, "Cannot respawn no spawn points", _health);
                if(point != null)
                    _health.transform.position = point.transform.position;
            }
        }
    }
}