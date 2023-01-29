using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Characters
{
    [Serializable]
    public class CameraAssignments : IPlayerDependencyResolver<SharedCamera>, IPlayerDependencyResolver<Camera>
    {
        [SerializeField] 
        enum CameraModes
        {
            SINGLE_PLAYER,
            SPLIT_SCREEN,
            MULTI_MONITOR
        }
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.OnValueChanged(nameof(OnModeChanged))]
        [Sirenix.OdinInspector.EnumPaging]
#endif
        [FoldoutGroup("Camera Mode")]
        [SerializeField] CameraModes cameraMode = CameraModes.SPLIT_SCREEN;

        [PropertyOrder(-1)]
        [ValidateInput(nameof(ValidateSharedCameras))]
        [SerializeField] private SharedCamera[] playerCameras;

        bool ValidateSharedCameras(SharedCamera[] cameras)
        {
            if(cameras.Length < 2) return false;
            for (int i = 0; i < cameras.Length; i++)
            {
                var c = cameras[i];
                if (c == null) return false;
            }
            return true;
        }
        
       [FoldoutGroup("Camera Mode")] [SerializeField] private PlayerCameras splitScreenCameras;
       [FoldoutGroup("Camera Mode")] [SerializeField] private PlayerCameras dualMonitorCameras;
       [FoldoutGroup("Camera Mode")] [SerializeField] private PlayerCameras singlePlayerCamera;


       private CameraModes _defaultMode;
       
        IEnumerable<(PlayerCameras, CameraModes)> GetAllPlayerCameras()
        {
            if(splitScreenCameras != null)
                yield return (splitScreenCameras, CameraModes.SPLIT_SCREEN);
            if(dualMonitorCameras != null)
                yield return (dualMonitorCameras, CameraModes.MULTI_MONITOR);
            if(singlePlayerCamera != null)
                yield return (singlePlayerCamera, CameraModes.SINGLE_PLAYER);
        }
        public PlayerCameras GetPlayerCameras()
        {
            switch (cameraMode)
            {
                case CameraModes.SPLIT_SCREEN:
                    return ActivateCamera(splitScreenCameras);
                case CameraModes.MULTI_MONITOR:
                    return ActivateCamera(dualMonitorCameras);
                default:
                    return ActivateCamera(singlePlayerCamera);
            }
        }
        PlayerCameras ActivateCamera(PlayerCameras playerCameras)
        {
            foreach (var allPlayerCamera in GetAllPlayerCameras())
            {
                allPlayerCamera.Item1.gameObject.SetActive(allPlayerCamera.Item1 == playerCameras);
            }
            return playerCameras;
        }


        void OnModeChanged()
        {
            switch (cameraMode)
            {
                case CameraModes.SPLIT_SCREEN:
                    ActivateCamera(splitScreenCameras);
                    break;
                case CameraModes.MULTI_MONITOR:
                    ActivateCamera(dualMonitorCameras);
                    break;
                default:
                    ActivateCamera(singlePlayerCamera);
                    break;
            }
        }

        public SharedCamera GetDependency(int playerNumber)
        {
            var sharedCamera = playerCameras[playerNumber];
            var cameras = GetPlayerCameras();
            sharedCamera.Value = cameras.GetCamera(playerNumber);
            return sharedCamera;
        }

        public IDisposable DisableMultiPlayerCameras()
        {
            _defaultMode = cameraMode;
            cameraMode = CameraModes.SINGLE_PLAYER;
            OnModeChanged();
            return Disposable.Create(EnableMultiPlayerCameras);
        }
        public void EnableMultiPlayerCameras()
        {
            cameraMode = _defaultMode;
            OnModeChanged();
        }

        Camera IPlayerDependencyResolver<Camera>.GetDependency(int playerNumber)
        {
            return GetPlayerCameras().GetDependency(playerNumber);
        }
    }
}