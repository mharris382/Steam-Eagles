﻿using System;
using System.Collections;
using System.Collections.Generic;
using Buildings.Messages;
using CoreLib;
using CoreLib.Entities;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildings.Rooms.Tracking
{
    public class RoomCinematicsController : MonoBehaviour
    {
        private CompositeDisposable _cd;
        
      [ShowInInspector,HideInEditorMode]  private RoomCameraLookup _cameraLookup;
      [ShowInInspector,HideInEditorMode]  private PCRoomTracker _pcRoomTracker;
      [ShowInInspector,HideInEditorMode]  private PCCamera[] _pcCameras;

        public class PCCamera : IDisposable
        {
            private readonly int _playerNumber;
            private readonly RoomCameraLookup _cameraLookup;
            private readonly PCRoomTracker.PC _pcRoomTracker;
            private CompositeDisposable _cd;

            public PCCamera(MonoBehaviour owner, int playerNumber,
                RoomCameraLookup cameraLookup, PCRoomTracker.PC pcRoomTracker)
            {
                _cd = new CompositeDisposable();
                _playerNumber = playerNumber;
                _cameraLookup = cameraLookup;
                _pcRoomTracker = pcRoomTracker;
                owner.StartCoroutine(DelaySubscription());
            }

            private IEnumerator DelaySubscription()
            {
                while(_cameraLookup.inited == false)
                {
                    Debug.Log($"P{_playerNumber} Camera Controller, Waiting on Camera Lookup");
                    yield return null;
                }
                _pcRoomTracker.OnRoomChanged.StartWith((null, _pcRoomTracker.PCRoom.Value)).Subscribe(t => OnPcRoomChanged(t.prevRoom, t.newRoom)).AddTo(_cd);
            }

            public void OnPcRoomChanged(Room prevRoom, Room newRoom)
            {
                SetRoomCameraEnabled(prevRoom, false);
                SetRoomCameraEnabled(newRoom, true);
            }

            public void SetRoomCameraEnabled(Room room, bool enabled)
            {
                var camera = _cameraLookup.GetPlayerVCam(room, _playerNumber);
                camera.SetActive(enabled);
            }

            public void Dispose()
            {
                _cd?.Dispose();
            }
        }
        
        [Inject]
        public void Install(RoomCameraLookup cameraLookup, PCRoomTracker pcRoomTracker)
        {
            _cameraLookup = cameraLookup;
            _pcRoomTracker = pcRoomTracker;
            Debug.Log("Installing RoomCinematicsController", this);
            SetupCameraTracker();
        }
        private void SetupCameraTracker()
        {
            _pcRoomTracker.onPCInstanceChanged.Subscribe(pc =>
            {
                if(_pcCameras[pc.PlayerNumber] != null) _pcCameras[pc.PlayerNumber].Dispose();
                _pcCameras[pc.PlayerNumber] = new PCCamera(this, pc.PlayerNumber, _cameraLookup, pc);
            }).AddTo(this);
        }

        private void Awake()
        {
            _cd = new CompositeDisposable();
            _pcCameras = new PCCamera[2];
        }
      
        
        private void OnDisable() => _cd?.Dispose();

        public bool HasResources() => _cameraLookup != null;
        
        
    }

    
}