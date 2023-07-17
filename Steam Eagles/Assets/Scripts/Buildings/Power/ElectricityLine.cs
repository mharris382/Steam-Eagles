using UnityEngine;


    [RequireComponent(typeof(LineRenderer))]
    public class ElectricityLine : MonoBehaviour
    {
        private LineRenderer _lineRenderer;
        public LineRenderer LineRenderer => _lineRenderer ? _lineRenderer : _lineRenderer = GetComponent<LineRenderer>();
    }
