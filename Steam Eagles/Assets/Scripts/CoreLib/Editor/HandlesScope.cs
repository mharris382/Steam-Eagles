
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CoreLib
{
    public class HandlesScope : IDisposable
    {
        private readonly Color _preColor;
        
        public HandlesScope(Color color)
        {
#if UNITY_EDITOR
            _preColor = Handles.color;
            Handles.color = color;
#endif
        }
        public void Dispose()
        {
            Handles.color = _preColor;
        }
    }
}