using UnityEngine;

namespace Buildings
{
    [RequireComponent(typeof(LineRenderer))]
    public class ElecLine : MonoBehaviour
    {
        [Range(1, 25)] public int subdivisions = 10;
        
        private LineRenderer _lineRenderer;
        public LineRenderer LineRenderer => _lineRenderer ? _lineRenderer : _lineRenderer = GetComponent<LineRenderer>();
        public void SetPoints(Vector2 pointsStart, Vector2 pointsEnd)
        {
            int cnt = subdivisions + 1;
            var points = new Vector3[cnt];
            for (int i = 0; i < cnt; i++)
            {
                points[i] = Vector3.Lerp(pointsStart, pointsEnd, i / (float)cnt);
            }
            this.LineRenderer.positionCount = 2;
        }
    }
}