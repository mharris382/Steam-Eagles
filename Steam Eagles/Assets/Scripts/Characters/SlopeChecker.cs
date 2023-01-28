using UnityEngine;

namespace Characters
{
    public class SlopeChecker
    {
        private readonly Rigidbody2D _rb;
        private readonly LayerMask _groundMask;
        private readonly CapsuleCollider2D _capsule;
        private Vector2 CapsuleSize => _capsule.size;

        public SlopeChecker(Rigidbody2D rigidbody2D, LayerMask groundMask)
        {
            this._rb = rigidbody2D;
            _groundMask = groundMask;
            this._capsule = _rb.GetComponent<CapsuleCollider2D>();
        }
    }
    
    [System.Serializable]
    public class SlopeCheckConfig
    {
        public float slopeCheckDistance = 0.5f;
        public float slopeCheckRadius = 0.5f;
    }
}