using UnityEngine;

namespace PhysicsFun.DynamicBlocks
{
    /// <summary>
    /// this class is instantiated from code by dynamic block manager
    /// </summary>
    public class DynamicBlock : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Collider2D[] _colliders;
        
        public Rigidbody2D Rigidbody2D => _rb;
        public Collider2D Collider2D => Colliders[0];
        public Collider2D[] Colliders => _colliders;
       
        public ScriptableObject blockID { get; set; }
        
        
        private void Awake()
        {
            _colliders = this.GetComponentsInChildren<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// sets the rigidbody to dynamic when enabled
        /// </summary>
        private void OnEnable()
        {
            Rigidbody2D.isKinematic = false;
            foreach (var col in _colliders)
            {
                col.enabled = true;
            }
        }
        /// <summary>
        /// disables collisions and sets the rigidbody to kinematic when disabled
        /// </summary>
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