using UnityEngine;

namespace Experimental
{
    public class GearBuilder : MonoBehaviour
    {
        public SpriteRenderer gearPreview;
        
        public Gear[] gearPrefabs;
        
        
        
        
    }

    
    [RequireComponent(typeof(Rigidbody2D),  typeof(CircleCollider2D))]
    public class GearAxel : MonoBehaviour
    {
        Rigidbody2D _rb;

        public Rigidbody2D rb => _rb ? _rb : _rb = GetComponent<Rigidbody2D>();
    }
}