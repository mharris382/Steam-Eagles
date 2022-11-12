using System;
using UnityEngine;

namespace Characters
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public class Pickup : MonoBehaviour
    {
        public Material selectedMaterial;
        private SpriteRenderer _spriteRenderer;

         
        
        
        private void Awake()
        {
            this._spriteRenderer = GetComponent<SpriteRenderer>();
            
        }
    }
    
    
}