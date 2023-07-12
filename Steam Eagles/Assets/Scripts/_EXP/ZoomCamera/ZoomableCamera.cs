using System;
using System.Collections;
using Cinemachine;
using CoreLib;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace _EXP.ZoomCamera
{
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class ZoomableCamera : MonoBehaviour
    {
        public float minZoom = 1;
        public float maxZoom = 10;
        public float smoothTime = 0.1f;
        
        private Coroutine _coroutine;
        private CinemachineVirtualCamera _vCam;
        private CinemachineFramingTransposer _transposer;
        private CinemachineBrain _cinemachineBrain;


        [Inject]
        public void Install(CoroutineCaller coroutineCaller)
        {
            _coroutineCaller = coroutineCaller;
        }
        

        private CinemachineVirtualCamera VCam => _vCam ? _vCam : _vCam = GetComponent<CinemachineVirtualCamera>();

        private float _velocity;
        private float _currentZoom;
        private float _targetZoom;
        private CoroutineCaller _coroutineCaller;
        private ZoomableCamera _other;
        private int GetPlayerIndex => (gameObject.layer == LayerMask.NameToLayer("P1") ||gameObject.layer == LayerMask.NameToLayer("TransparentFX")) ? 0 : 1;

        private void Awake()
        {
            _vCam = GetComponent<CinemachineVirtualCamera>();
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().Subscribe(Assigned).AddTo(this);
        }

        private void OnEnable()
        {
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().TakeUntilDisable(this).Subscribe(Assigned).AddTo(this);
        }

        //called when player input is assigned to a character and a camera
        void Assigned(CharacterAssignedPlayerInputInfo info)
        {
            _cinemachineBrain = info.camera.GetComponent<CinemachineBrain>();
            _cinemachineBrain.m_CameraCutEvent.AddListener(CutTo);
        }

        public void ReassignFrom(GameObject other)
        {
            var zoomableCamera = other.GetComponent<ZoomableCamera>();
            _other = zoomableCamera;
            _coroutineCaller = zoomableCamera._coroutineCaller;
            _cinemachineBrain = zoomableCamera._cinemachineBrain;
            _vCam = GetComponent<CinemachineVirtualCamera>();
            if (_coroutine != null && _coroutineCaller != null)
            {
                _coroutineCaller.StopCoroutine(_coroutine);
                _coroutine = null;
            }
            CutTo(_cinemachineBrain);
        }
        
        //called when cinematic camera cuts to this camera
        private void CutTo(CinemachineBrain arg0)
        {
            bool isActive = arg0.ActiveVirtualCamera.VirtualCameraGameObject.name == gameObject.name;
            bool isRunning = CheckRunning();
            if (!isActive && isRunning)
            {
                _coroutineCaller. StopCoroutine(_coroutine);
                _coroutine = null;
            }
            else if(isActive && !isRunning)
                StartInputCoroutine();
        }

        
        IEnumerator ProcessInput()
        {
            while (true)
            {
                if (TryGetPlayerInput(out var playerInput))
                {
                    var value = playerInput.actions["Zoom"].ReadValue<float>();
                    _targetZoom = _currentZoom + value * Time.deltaTime;
                    _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
                    _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _velocity, smoothTime);
                    SetSize(_currentZoom);
                }
                yield return null;
            }
        }

        public void LateUpdate()
        {
            if (TryGetPlayerInput(out var playerInput))
            {
                var value = playerInput.actions["Zoom"].ReadValue<float>();
                _targetZoom = _currentZoom + value * Time.deltaTime;
                _targetZoom = Mathf.Clamp(_targetZoom, minZoom, maxZoom);
                _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _velocity, smoothTime);
                SetSize(_currentZoom);
            }
            return;
            if (gameObject.layer == LayerMask.NameToLayer("TransparentFX"))
            {
                var parent = transform.parent;
                for (int i = 0; i < parent.childCount; i++)
                {
                    var t = parent.GetChild(i);
                    if (t == transform) continue;
                    var zoomableCamera = t.GetComponent<ZoomableCamera>();
                    if(zoomableCamera != null)
                        zoomableCamera.ReassignFrom(gameObject);
                }
                return;
            }
            if (_cinemachineBrain == null && _other != null) _cinemachineBrain = _other._cinemachineBrain;
            if (_coroutineCaller == null && _other != null) _coroutineCaller = _other._coroutineCaller;
            if(_cinemachineBrain == null) return;
            CutTo(_cinemachineBrain);
        }

        private bool CheckRunning()
        {
            return _coroutine != null;
        }

        private void StartInputCoroutine()
        {
            _coroutine = _coroutineCaller.StartCoroutine(ProcessInput());
        }

        bool TryGetPlayerInput(out PlayerInput playerInput)
        {
             
             if (GameManager.Instance.PlayerHasJoined(GetPlayerIndex))
             {
                 playerInput = GameManager.Instance.GetPlayerDevice(GetPlayerIndex).GetComponent<PlayerInput>();
                 return true;
             }
            playerInput = null;
            return false;
        }
        
        void SetSize(float size)
        {
            _vCam = GetComponent<CinemachineVirtualCamera>();
            _vCam.m_Lens.OrthographicSize = size;
        }
    }
}