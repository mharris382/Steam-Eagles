using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib
{
    [InfoBox("Helper component which copies all camera setups for use with split screen camera.  This is required for cinemachine to function properly")]
    public class GameplayCamera : MonoBehaviour
    {
        public static event Action<GameObject> CameraCreated;
        private void Start()
        {
            //CreateCopy();
        }

        public void CreateCopy()
        {
            var copy = Instantiate(this, transform.position, transform.rotation, transform.parent);
            var cameraName = this.name;
            
            this.name = $"{cameraName} (P1)";
            this.gameObject.layer = LayerMask.NameToLayer("P1");
            
            copy.name = $"{cameraName} (P2)";
            copy.gameObject.layer = LayerMask.NameToLayer("P2");
            CameraCreated?.Invoke(copy.gameObject);
        }
    }
}