using UnityEngine;

namespace Utilities
{
    public class MoveHelper : MonoBehaviour
    {
        public Rigidbody2D rb;
        public Transform resetPoint;
        public void MoveDown(float amount)
        {
            if (resetPoint != null)
            {
                rb.position = resetPoint.position;
            }
            rb.velocity = Vector2.down * amount;
        }

        public void MoveUp(float amount)
        {
            if (resetPoint != null)
            {
                rb.position = resetPoint.position;
            }
            rb.velocity = Vector2.up * amount;
        }

        public void StopMoving()
        {
            if (resetPoint != null)
            {
                rb.position = resetPoint.position;
            }
            rb.velocity = Vector2.zero;
        }
        
    }
}