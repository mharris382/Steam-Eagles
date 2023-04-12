using System;
using Cinemachine;
using CoreLib;
using CoreLib.SharedVariables;
using UnityEngine;

namespace Characters.Cameras
{
    public class CameraController : MonoBehaviour
    {
        public bool multiMonitorSplitScreen = true;
        
        public SharedTransform p1Transform;
        public SharedTransform p2Transform;
        
        public Camera p1Camera;
        public Camera p2Camera;

        public SharedCamera p1ActiveCamera;
        public SharedCamera p2ActiveCamera;
        
        public CinemachineVirtualCamera p1DefaultVirtualCamera;
        public CinemachineVirtualCamera p2DefaultVirtualCamera;

        public void Awake()
        {
            p1Transform.onValueChanged.AddListener(t => p1DefaultVirtualCamera.Follow = t);
            p1DefaultVirtualCamera.gameObject.layer = LayerMask.NameToLayer("P1");
            
            p2Transform.onValueChanged.AddListener(t => p2DefaultVirtualCamera.Follow = t);
            p2DefaultVirtualCamera.gameObject.layer = LayerMask.NameToLayer("P2");
            
            
        }

        private void Start()
        {
            if (multiMonitorSplitScreen && Display.displays.Length > 1)
            {
                Display.displays[0].Activate();
                p1Camera.targetDisplay = 0;
                
                Display.displays[1].Activate();
                p2Camera.targetDisplay = 1;
            }
            else
            {
                // single monitor SplitScreen
                //p1Camera.rect = new Rect(0, 0, 0.5f, 1);
                //p2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
            }
            
            p2Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("P1"));
            p1Camera.cullingMask &= ~(1 << LayerMask.NameToLayer("P2"));
            
            p1ActiveCamera.Value = p1Camera;
            p2ActiveCamera.Value = p2Camera;
        }
    }
}