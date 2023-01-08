using System;
using UnityEngine;

namespace GasSim.GPUSim
{
    public class GPUSim : MonoBehaviour
    {

        public BoxCollider2D collider2D;
        public Vector2 cellSize = Vector2.one;

        
        
        private GPUSimState _state;
        

        private void Awake()
        {
            _state = new GPUSimState(collider2D, cellSize);
            _state.Init();
        }
    }
}