using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(CameraSizeHelper))]
    public class CameraSizeHelper : MonoBehaviour
    {
        private Camera _camera;
        private Rect _size;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _size = _camera.rect;
        }
        
        public void FullScreen()
        {
            _camera.rect = new Rect(0,0,1,1);
        }

        public void ResetToDefault()
        {
            _camera.rect = _size;
        }
    }
}