using System;
using System.Collections.Generic;
using UnityEngine;

//TODO: settings menu feature, allow players to choose which additional monitor to use in case of >2 monitors
namespace Players
{
    [Serializable]
    public class CameraAssignments 
    {   
        [SerializeField] private Camera[] cameras;

        public Camera this[int playerNumber]
        {
            get
            {
                switch(playerNumber){
                    case 0:
                    case 1:
                        GetPCam(playerNumber);
                        throw new NotImplementedException();
                    default:
                        throw new IndexOutOfRangeException("Only supports up to 2 local players. index is zero based");
                }
            }
        }

        public IEnumerable<Camera> GetAssignableCameras()
        {
            yield return GetPCam(0);
            yield return GetPCam(1);
        }

    
        private bool HasP1Cam() => HasPCam(0);
        private bool HasP2Cam() => HasPCam(1);
        private bool HasPCam(int playerNumber) => cameras != null && cameras.Length > playerNumber && cameras[playerNumber] != null;
        private Camera GetPCam(int playerNumber) => HasPCam(playerNumber) ? cameras[playerNumber] : null;


        private Camera CreatePCam(int playerNumber)
        {
            throw new NotImplementedException();
        }
    }
}
