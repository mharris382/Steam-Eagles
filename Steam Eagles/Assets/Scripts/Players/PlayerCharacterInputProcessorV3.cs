using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Game;
using Players.Shared;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Players
{
    public class PlayerCharacterInputProcessorV3 : IDisposable
    {
        internal class InspectInput
        {
            PlayerCharacterInput _playerCharacterInput;
            
            [ShowInInspector]
            public int PlayerNumber => _playerCharacterInput.playerNumber;

            [HorizontalGroup("h1")]
            [VerticalGroup("h1/Actual")]
            [BoxGroup("h1/Actual/Processor")]
            [ShowInInspector]
            public PlayerInput PlayerInput
            {
                get => _playerCharacterInput._assignedPlayerInput.Value;
            }
            
            [VerticalGroup("h1/Actual")]
            [BoxGroup("h1/Actual/Processor")]
            [ShowInInspector]
            public CharacterState CharacterState
            {
                get => _playerCharacterInput._assignedCharacterState.Value;
            }

            [VerticalGroup("h1/Actual")]
            [BoxGroup("h1/Actual/Processor")]
            [ShowInInspector]
            public Vector2 MoveInput
            {
                get => _playerCharacterInput.moveInput;
            }
            
            [VerticalGroup("h1/Global")]
            [BoxGroup("h1/Global/Game")]
            [ShowInInspector]
            public PlayerInput GlobalPlayerInput
            {
                get
                {
                    if (GameManager.Instance.PlayerHasJoined(PlayerNumber))
                    {
                        return GameManager.Instance.GetPlayerDevice(PlayerNumber).GetComponent<PlayerInput>();
                    }

                    return null;
                }
            }
            [BoxGroup("h1/Global/Game")]
            [VerticalGroup("h1/Global")]
            [ShowInInspector]
            public CharacterState GlobalCharacterState
            {
                get
                {
                    if (_playerCharacterInput._assignedCharacterState.Value != null)
                    {
                        return _playerCharacterInput._assignedCharacterState.Value;
                    }
                    if (GameManager.Instance.PlayerHasCharacterAssigned(PlayerNumber) &&
                        SceneManager.GetActiveScene().buildIndex != 0)
                    {
                        var go = GameObject.FindGameObjectWithTag(
                            GameManager.Instance.GetPlayerCharacterName(PlayerNumber));
                        if (go != null)
                        {
                            return go.GetComponent<CharacterState>();
                        }
                    }

                    return null;
                }
            }
            
            internal InspectInput(PlayerCharacterInput playerCharacterInput)
            {
                _playerCharacterInput = playerCharacterInput;
            }
        }
        internal class PlayerCharacterInput : IDisposable
        {
            public readonly int playerNumber;
            private readonly PlayerCharacterInputProcessorV3 _inputProcessorManager;
            private readonly bool _debug;

            internal ReactiveProperty<PlayerInput> _assignedPlayerInput;
            internal ReactiveProperty<CharacterState> _assignedCharacterState;
            private IDisposable _disposable;

            public PlayerCharacterInput(int playerNumber,
                Subject<Unit> processorManager,
                PlayerCharacterInputProcessorV3 inputProcessorManager,
                bool debug = false)
            {
                this.playerNumber = playerNumber;
                _inputProcessorManager = inputProcessorManager;
                _debug = debug;
                _assignedCharacterState = new ReactiveProperty<CharacterState>();
                _assignedPlayerInput = new ReactiveProperty<PlayerInput>();
                var cd = new CompositeDisposable();

                _assignedPlayerInput.AddTo(cd);
                _assignedCharacterState.AddTo(cd);
                processorManager.Subscribe(_ => Debug.Log("Updating!")).AddTo(cd);
                
                MessageBroker.Default.Receive<PlayerDeviceJoined>().Where(t => t.Index == playerNumber)
                    .Subscribe(t => _assignedPlayerInput.Value = t.PlayerInput.GetComponent<PlayerInput>()).AddTo(cd);
                
                MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>()
                    .Where(t => t.playerNumber == playerNumber)
                    .Subscribe(info =>
                    {
                        Debug.Assert(info.characterState != null, "info.characterState != null");
                        Debug.Assert(info.inputGo != null, "info.inputGo != null");
                        _assignedCharacterState.Value = info.characterState as CharacterState;
                        _assignedPlayerInput.Value = info.inputGo.GetComponent<PlayerInput>();
                    }).AddTo(cd);
                    
                    var zipped =_assignedPlayerInput
                        .ZipLatest(_assignedCharacterState,
                        (input, state) => (input, state, input != null && state != null));
                    
                    zipped.Select(t => t.Item3 ? 
                            processorManager : 
                            Observable.Empty<long>().AsUnitObservable()).Switch()
                    .Subscribe(ProcessInputOnUpdate)
                    .AddTo(cd);
                    
                     

                    zipped.Select(t => !t.Item3 ? Observable.EveryUpdate() : Observable.Empty<long>()).Switch()
                        .Subscribe(_ =>
                        {
                            if (_assignedPlayerInput.Value==null && GameManager.Instance.PlayerHasJoined(playerNumber))
                            {
                                _assignedPlayerInput.Value = GameManager.Instance.GetPlayerDevice(playerNumber).GetComponent<PlayerInput>();
                            }

                            if (_assignedCharacterState.Value==null && GameManager.Instance.PlayerHasCharacterAssigned(playerNumber))
                            {
                                var go = GameObject.FindGameObjectWithTag(
                                    GameManager.Instance.GetPlayerCharacterName(playerNumber));
                                if (go != null)
                                {
                                    _assignedCharacterState.Value = go.GetComponent<CharacterState>();
                                }
                            }
                        });
                
                _disposable = cd;
                
                if (_assignedPlayerInput.Value==null && GameManager.Instance.PlayerHasJoined(playerNumber))
                {
                    _assignedPlayerInput.Value = GameManager.Instance.GetPlayerDevice(playerNumber).GetComponent<PlayerInput>();
                }
                if (_assignedCharacterState.Value==null && GameManager.Instance.PlayerHasCharacterAssigned(playerNumber))
                {
                    var go = GameObject.FindGameObjectWithTag(
                        GameManager.Instance.GetPlayerCharacterName(playerNumber));
                    if (go != null)
                    {
                        _assignedCharacterState.Value = go.GetComponent<CharacterState>();
                    }
                }
            }

            public void Dispose()
            {
                _disposable?.Dispose();
            }

            public Vector2 moveInput;

            void ProcessInputOnUpdate(Unit _)
            {
                if (_debug)
                    Debug.Log($"Processing player {playerNumber} input for character {_assignedCharacterState.Value.name}");
                var characterState = _assignedCharacterState.Value;
                var playerInput = _assignedPlayerInput.Value;
                var toolState = characterState.Tool;
                
                bool usingKeyboardMouseInput = playerInput.currentControlScheme.Contains("Keyboard");
            
                moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
                var x = playerInput.actions["Move X"].ReadValue<float>();
                var y = playerInput.actions["Move Y"].ReadValue<float>();
                moveInput = new Vector2(x, y);
                var aimInput = playerInput.actions["Aim"].ReadValue<Vector2>();
                var jumpAction = playerInput.actions["Jump"];

                bool jumpPressed = jumpAction.WasPressedThisFrame();
                bool jumpHeld = jumpAction.IsPressed();
                if (moveInput.y < -0.5f)
                {
                    characterState.DropPressed = jumpHeld;
                    characterState.JumpPressed = characterState.JumpHeld = false;
                }
                else
                {
                    characterState.DropPressed = false;
                    characterState.JumpPressed = jumpPressed;
                    characterState.JumpHeld = jumpHeld;
                }

                characterState.MoveInput = moveInput;
                toolState.Inputs.AimInput = aimInput;
            
                toolState.Inputs.CurrentInputMode = usingKeyboardMouseInput ? InputMode.KeyboardMouse : InputMode.Gamepad;
            }
        }

        private PlayerCharacterInput[] _processors;
        private Dictionary<int, CharacterAssignedPlayerInputInfo> _playerInputMap;
        private CompositeDisposable _compositeDisposable;
        
        
        public PlayerCharacterInputProcessorV3(Subject<Unit> subject, bool debug = false)
        {
            _compositeDisposable = new CompositeDisposable();
            _playerInputMap = new Dictionary<int, CharacterAssignedPlayerInputInfo>();
            _processors = new PlayerCharacterInput[2] {
                new PlayerCharacterInput(0, subject,this, debug),
                new PlayerCharacterInput(1, subject,this, debug)
            };
            if (debug)
            {
                #if UNITY_EDITOR
                OdinEditorWindow.InspectObject(new InspectInput(_processors[0]));
                OdinEditorWindow.InspectObject(new InspectInput(_processors[1]));
                #endif
            }

            
            foreach (var playerCharacterInput in _processors) playerCharacterInput.AddTo(_compositeDisposable);
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().Subscribe(OnCharacterAssignedPlayerInput).AddTo(_compositeDisposable);
        }

        void OnCharacterAssignedPlayerInput(CharacterAssignedPlayerInputInfo inputInfo)
        {
            if(!_playerInputMap.ContainsKey(inputInfo.playerNumber))
            {
                _playerInputMap.Add(inputInfo.playerNumber, inputInfo);
            }
            else
            {
                _playerInputMap[inputInfo.playerNumber] = inputInfo;
            }
        }
        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }



    
}