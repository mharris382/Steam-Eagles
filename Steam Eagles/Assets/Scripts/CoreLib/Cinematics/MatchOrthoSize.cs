using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoreLib.Cinematics
{
    [ExecuteAlways]
    [RequireComponent(typeof(Camera))]
    public class MatchOrthoSize : MonoBehaviour
    {
        
        [Required] public Camera matchTarget;
        
        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;
        }

        private void Update()
        {
            if(matchTarget == null)return;
            _camera.orthographicSize = matchTarget.orthographicSize;
        }
    }
}