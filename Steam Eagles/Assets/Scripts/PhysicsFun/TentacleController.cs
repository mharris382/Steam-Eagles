using UnityEngine;

namespace PhysicsFun
{
    [ExecuteAlways]
    public class TentacleController : MonoBehaviour
    {
        [SerializeField] private TentacleAngleLimits angleLimits;
        
        [System.Serializable]
        public class TentacleAngleLimits
        {
            public bool useAngleLimits = true;
            public Vector2 angleRange = new Vector2(-90, 90);
            
            public bool useOffsetLimitsX = false;
            public Vector2 xOffsetRange = new Vector2(-1, 1);
            
            public bool useOffsetLimitsY = false;
            public Vector2 yOffsetRange = new Vector2(-2, 2);

            public void DrawGizmos(Transform t)
            {
                if (useAngleLimits)
                {
                    DrawAngleLimits(t.position, t.right);
                }

                if (useOffsetLimitsX)
                {
                    DrawOffsetLimitsX(t.position, t.right);
                }
                if(useOffsetLimitsY)
                {
                    DrawOffsetLimitsY(t.position, t.up);
                }
            }
            
            const float TANGENT_LINE_LENGTH = 0.125f;

            private void DrawOffsetLimitsY(Vector3 tPosition, Vector3 tRight)
            {
                var color = Color.green;
                color.a = 0.75f;
                Gizmos.color = color;
                var minPosition = tPosition + tRight * xOffsetRange.x;
                var maxPosition = tPosition + tRight * xOffsetRange.y;
                Gizmos.DrawLine(minPosition,maxPosition );
                var tangent = Vector3.Cross(tRight, Vector3.forward);
                Gizmos.DrawLine(minPosition - tangent * TANGENT_LINE_LENGTH, minPosition + tangent * TANGENT_LINE_LENGTH);
                Gizmos.DrawLine(maxPosition - tangent * TANGENT_LINE_LENGTH, maxPosition + tangent * TANGENT_LINE_LENGTH);
            }

            private void DrawOffsetLimitsX(Vector3 tPosition, Vector3 tUp)
            {
                var color = Color.green;
                color.a = 0.75f;
                Gizmos.color = color;
                var minPosition = tPosition + tUp * yOffsetRange.x;
                var maxPosition = tPosition + tUp * yOffsetRange.y;
                Gizmos.DrawLine(minPosition, maxPosition);
                var tangent = Vector3.Cross(tUp, Vector3.forward);
                Gizmos.DrawLine(minPosition - tangent * TANGENT_LINE_LENGTH, minPosition + tangent * TANGENT_LINE_LENGTH);
                Gizmos.DrawLine(maxPosition - tangent * TANGENT_LINE_LENGTH, maxPosition + tangent * TANGENT_LINE_LENGTH);
            }


            private void DrawAngleLimits(Vector3 tPosition, Vector3 tRight)
            {
                var color = Color.green;
                color.a = 0.5f;
                Gizmos.color = color;
                var minAngle = angleRange.x;
                var maxAngle = angleRange.y;
                var maxDir = Quaternion.Euler(0, 0, maxAngle) *  tRight;
                var minDir = Quaternion.Euler(0, 0, minAngle) *  tRight;
                Gizmos.DrawRay(tPosition,maxDir* 2);
                Gizmos.DrawRay(tPosition,minDir* 2);
                
            }

            
        }

        
        
        
        
        
        public void OnDrawGizmos()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                angleLimits.DrawGizmos(child);
            }
        }
        
    }
}