using CoreLib;
using UniRx;
//using UniRx;
using UnityEngine;

namespace PhysicsFun.Airships
{
    public class PilotCamera : MonoBehaviour
    {
        public string characterName = "Builder";
        public GameObject cameraObject;
        
        void Awake()
        {
            if (cameraObject == null)
                cameraObject = gameObject;
            Observable.Merge(
                    MessageBroker.Default.Receive<AirshipPilotChangedInfo>().Where(t => t.prevPilotName == characterName && t.newPilotName != characterName).Select(_ => false),
                    MessageBroker.Default.Receive<AirshipPilotChangedInfo>().Where(t => t.newPilotName == characterName).Select(_ => true))
                .Subscribe(cameraOn => cameraObject.SetActive(cameraOn))
                .AddTo(this);
            cameraObject.SetActive(false);
        }
        
    }
}