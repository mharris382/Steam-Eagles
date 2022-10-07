using System;
using UnityEngine;

namespace Experimental.StructureBufferExperiment1
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SBE1_Cell : MonoBehaviour, ICell
    {
        public Color Color
        {
            set
            {
                _sr.color = value;
            }
            get
            {
                return _sr.color;
            }
        }

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }
    }
}