using UnityEngine;

namespace AI.Enemies
{
    public class EnvironmentChecks : MonoBehaviour
    {
        public GroundCheckConfig leftWallCheck;
        public GroundCheckConfig rightWallCheck;
        public LedgeCheckConfig leftEdgeCheck;
        public LedgeCheckConfig rightEdgeCheck;
        
        public bool CheckLeftWall() => leftWallCheck.CheckGrounded();
        public bool CheckRightWall() => rightWallCheck.CheckGrounded();
        public bool CheckLeftEdge() => leftEdgeCheck.CheckGrounded() || leftEdgeCheck.CheckForDroppableLedge();
        public bool CheckRightEdge() => rightEdgeCheck.CheckGrounded() || rightEdgeCheck.CheckForDroppableLedge();


        public bool CanMoveLeft()
        {
            return CheckLeftEdge() && !CheckLeftWall();
        }

        public bool CanMoveRight()
        {
            return CheckRightEdge() && !CheckRightWall();
        }

        public bool CanMove(float direction)
        {
            if (direction > 0) return CanMoveRight();
            if (direction < 0) return CanMoveLeft();
            return false;
        }
    }
}