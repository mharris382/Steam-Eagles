using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Enemies
{
    public class FlipHorizontal : MonoBehaviour
    {
        public bool invertDirection;
        public void SetFacingRight(bool faceRight)
        {
            faceRight = invertDirection ? !faceRight : faceRight;
            transform.localScale = new Vector3(
                1,
                faceRight ? 1 :  -1, 
                1);
        }
        
        public void SmoothSetFacingRight(bool faceRight)
        {
            faceRight = invertDirection ? !faceRight : faceRight;
            transform.localScale = new Vector3(
                1,
                faceRight ? 1 :  -1, 
                1);
        } 
    }
}

