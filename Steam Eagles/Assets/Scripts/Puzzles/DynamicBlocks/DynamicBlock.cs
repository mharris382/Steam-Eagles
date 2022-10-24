using UnityEngine;

namespace Puzzles
{
    /// <summary>
    /// this class is instantiated from code by dynamic block manager
    /// </summary>
    public class DynamicBlock : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Collider2D[] _colliders;

        public Rigidbody2D Rigidbody2D => _rb;

        private void Awake()
        {
            _colliders = this.GetComponentsInChildren<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            Rigidbody2D.isKinematic = false;
            foreach (var col in _colliders)
            {
                col.enabled = true;
            }
        }

        private void OnDisable()
        {
            Rigidbody2D.isKinematic = true;
            foreach (var col in _colliders)
            {
                col.enabled = false;
            }
        }
    }
}