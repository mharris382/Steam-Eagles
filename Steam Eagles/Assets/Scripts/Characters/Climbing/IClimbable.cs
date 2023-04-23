

using UnityEngine;

namespace Characters
{
    public interface IClimbable
    {
        Rigidbody2D Rigidbody { get; }
        Vector2 MinClimbLocalPosition { get; }
        Vector2 MaxClimbLocalPosition { get; }
    }
}