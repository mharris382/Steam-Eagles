using UnityEngine;

namespace CoreLib.Interfaces
{
    public interface IClimbable
    {
        Rigidbody2D Rigidbody { get; }
        Vector2 MinClimbLocalPosition { get; }
        Vector2 MaxClimbLocalPosition { get; }
    }
    
    public interface IClimbableFactory
    {
        bool TryGetClimbable(Vector2 climberPosition, out IClimbable climbable, float maxDistance);
    }

}