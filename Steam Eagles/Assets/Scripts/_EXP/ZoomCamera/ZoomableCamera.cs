using System;
using System.Collections;
using Cinemachine;
using CoreLib;
using Game;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

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

        

        private CinemachineVirtualCamera VCam => _vCam ? _vCam : _vCam = GetComponent<CinemachineVirtualCamera>();

        private float _velocity;
        private float _currentZoom;
        private float _targetZoom;
        
        private int GetPlayerIndex => gameObject.layer == LayerMask.NameToLayer("P1") ? 0 : 1;

        private void Awake()
        {
            _vCam = GetComponent<CinemachineVirtualCamera>();
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>().Subscribe(Assigned).AddTo(this);
        }

        //called when player input is assigned to a character and a camera
        void Assigned(CharacterAssignedPlayerInputInfo info)
        {
            _cinemachineBrain = info.camera.GetComponent<CinemachineBrain>();
            _cinemachineBrain.m_CameraCutEvent.AddListener(CutTo);
        }

        //called when cinematic camera cuts to this camera
        private void CutTo(CinemachineBrain arg0)
        {
            bool isActive = arg0.ActiveVirtualCamera.VirtualCameraGameObject == gameObject;
            bool isRunning = CheckRunning();
            if (!isActive && isRunning)
            {
                StopCoroutine(_coroutine);
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

        private void Update()
        {
            if(_cinemachineBrain == null) return;
            CutTo(_cinemachineBrain);
        }

        private bool CheckRunning()
        {
            return _coroutine != null;
        }

        private void StartInputCoroutine()
        {
            _coroutine = StartCoroutine(ProcessInput());
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
            _vCam.m_Lens.OrthographicSize = size;
        }
    }
}