using System;
using UnityEngine;

namespace Utilities
{
    [RequireComponent(typeof(LineRenderer))]
    public class LineOnChildren : MonoBehaviour
    {
        private LineRenderer _lr;
        private LineRenderer lr => _lr ? _lr : _lr = GetComponent<LineRenderer>();
        private void Update()
        {
            var childCount = transform.childCount;
            lr.positionCount = childCount;
            for (var i = 0; i < childCount; i++)
            {
                lr.SetPosition(i, transform.GetChild(i).position);
            }
        }
    }
}