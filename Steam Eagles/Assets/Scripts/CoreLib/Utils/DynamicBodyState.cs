using System;
using UnityEngine;

namespace CoreLib
{
    
    [Obsolete("Replaced by StructureState")]
    public class DynamicBodyState : MonoBehaviour
    {
        public Vector2 MovingObjectVelocity
        {
            get
            {
                if (RoomBody != null)
                {
                    return RoomBody.velocity;
                }
                else if (BuildingBody != null)
                {
                    return BuildingBody.velocity;
                }
                else
                {
                    return Vector2.zero;
                }
            }
        }
        public Rigidbody2D RoomBody { get; set; }
        public Rigidbody2D BuildingBody { get; set; }
    }
}