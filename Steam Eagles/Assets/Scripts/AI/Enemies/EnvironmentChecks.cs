using UnityEngine;

namespace AI.Enemies
{
    public class EnvironmentChecks : MonoBehaviour
    {
        public GroundCheckConfig leftWallCheck;
        public GroundCheckConfig rightWallCheck;
        public GroundCheckConfig leftEdgeCheck;
        public GroundCheckConfig rightEdgeCheck;
        
        public bool CheckLeftWall() => leftWallCheck.CheckGrounded();
        public bool CheckRightWall() => rightWallCheck.CheckGrounded();
        public bool CheckLeftEdge() => leftEdgeCheck.CheckGrounded();
        public bool CheckRightEdge() => rightEdgeCheck.CheckGrounded();
    }
}