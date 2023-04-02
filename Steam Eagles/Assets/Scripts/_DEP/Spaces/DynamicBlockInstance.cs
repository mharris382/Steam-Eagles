using CoreLib;
using UnityEngine;

namespace Spaces
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class DynamicBlockInstance : MonoBehaviour, IBlockID
    {
        private Rigidbody2D _rigidbody2D;
        public Rigidbody2D Rigidbody2D => _rigidbody2D == null ? (_rigidbody2D = GetComponent<Rigidbody2D>()) : _rigidbody2D;

        [SerializeField]
        private DynamicBlock _block;
        public DynamicBlock Block
        {
            get => _block;
            set
            {
                _block = value;
            
           
            }
        }


        public string BlockName => Block.blockName;
    }
}