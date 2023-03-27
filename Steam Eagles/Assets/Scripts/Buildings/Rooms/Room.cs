using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    [Serializable]
    public class Room : MonoBehaviour
    {
        public GameObject roomCamera;
        public Color roomColor = Color.red;
        public Bounds roomBounds = new Bounds(Vector3.zero, Vector3.one);

        [ToggleGroup(nameof(isDynamic))]public bool isDynamic;
        [ToggleGroup(nameof(isDynamic))] public Rigidbody2D dynamicBody;
        
        public Bounds Bounds
        {
            get => roomBounds;
            set => roomBounds = value;
        }


        
        public void SetCameraActive(bool active)
        {
            if(roomCamera != null)
                roomCamera.SetActive(active);
        }
    }
}