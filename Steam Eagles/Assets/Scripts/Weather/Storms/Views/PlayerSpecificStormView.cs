using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class PlayerSpecificStormView : MonoBehaviour
    {
         [SerializeField]
        private List<PlayerSpecificObject> playerSpecificStormObjects = new List<PlayerSpecificObject>();

        private Camera _playerCamera;
        
        public void SetPlayerIndex(int playerIndex)
        {
            int layer = 0;
            switch (playerIndex)
            {
                case 0:
                    layer = LayerMask.NameToLayer("P1");
                    break;
                case 1:
                    layer = LayerMask.NameToLayer("P2");
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }

            foreach (var playerSpecificStormObject in playerSpecificStormObjects)
            {
                playerSpecificStormObject.SetLayer(layer);
            }
        }
         public void SetPlayerCamera(Camera camera) => _playerCamera = camera;

        public void Update()
        {
            if (_playerCamera == null) return;
            foreach (var playerSpecificStormObject in playerSpecificStormObjects)
                playerSpecificStormObject.Update(_playerCamera);
        }

        public class Factory : PlaceholderFactory<PlayerSpecificStormView> { }

        public void Assign(int playerNumber, Camera playerCamera)
        {
            SetPlayerIndex(playerNumber);
            SetPlayerCamera(playerCamera);
        }
        
        [Serializable]
        public class PlayerSpecificObject
        {
            
            [Required, ChildGameObjectsOnly] public GameObject followerObject;

            public bool followCamera = true;
            public Vector3 offset = new Vector3(0, 1, 1);
            
            public void SetLayer(int layer)
            {
                followerObject.layer = layer;
            }

            public void Update(Camera playerCam)
            {
                if (followCamera)
                {
                    var position = playerCam.transform.position;
                    followerObject.transform.position = position + offset;
                }
            }
        }
    }
}