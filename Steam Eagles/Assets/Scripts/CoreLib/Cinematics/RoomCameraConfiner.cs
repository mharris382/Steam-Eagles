using UnityEngine;

namespace CoreLib.Cinematics
{
    [RequireComponent(typeof(BoxCollider))]
    public class RoomCameraConfiner : MonoBehaviour
    {
        private BoxCollider _boxCollider;
        public BoxCollider boxCollider => _boxCollider ? _boxCollider : _boxCollider = GetComponent<BoxCollider>();
    }
}