using System;
using UnityEngine;

namespace UI.Utilities
{
    public class UIFixScale : MonoBehaviour
    {
        private Vector3 _scale;

        private void Awake()
        {
            _scale = transform.localScale;
        }

        public void ResetScale()
        {
            transform.localScale = _scale;
        }
    }
}