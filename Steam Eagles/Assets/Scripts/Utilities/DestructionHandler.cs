using System;
using UnityEngine;

namespace Utilities
{
    public class DestructionHandler : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshFilter meshFilter => _meshFilter ? _meshFilter : _meshFilter = GetComponent<MeshFilter>();

        public GameFX destructionFX;
        public bool destroyOnDestruction = true;
        public bool disableRenderer = true;
        public bool disablePhysics = true;
        public void HandleDestruction2D(Rigidbody2D rigidbody2D, SpriteRenderer spriteRenderer)
        {
            if (destroyOnDestruction)
            {
                var position = rigidbody2D.transform.position;
                var rotation = rigidbody2D.rotation;
                if (destructionFX)
                {
                    destructionFX.SpawnEffectFrom(position, rotation);
                }
                Destroy(rigidbody2D.gameObject);
            }
            else
            {
                if(disablePhysics) rigidbody2D.isKinematic =true;
                if(disableRenderer) spriteRenderer.enabled = false;
                if (destructionFX)
                {
                    destructionFX.SpawnEffectFrom(spriteRenderer.transform);
                }
            }
        }
    }
}