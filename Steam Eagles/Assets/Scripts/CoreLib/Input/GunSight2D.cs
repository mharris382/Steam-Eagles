using UnityEngine;

namespace Utilities
{
    [ExecuteAlways]
    public class GunSight2D : MonoBehaviour
    {
        public Gun2D gun2D;
        public LineRenderer lineRenderer;
        
        public LayerMask layerMask ;
        private void Update()
        {
            if(gun2D == null || lineRenderer == null)
                return;
            lineRenderer.SetPosition(0, gun2D.FirePoint.position);
            var hit = Physics2D.Raycast(gun2D.FirePoint.position, gun2D.FirePoint.right, 100, layerMask);
            if (hit)
            {
                lineRenderer.SetPosition(1, hit.point);
            }
            else
            {
                lineRenderer.SetPosition(1, gun2D.FirePoint.position + gun2D.FirePoint.right * 100);    
            }
        }
    }
}