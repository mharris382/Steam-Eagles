using UnityEngine;

namespace Characters
{
    public interface IClimber
    {
        Rigidbody2D Rigidbody { get; }

        // ReSharper disable once InconsistentNaming
        GameObject gameObject { get; }
        
        
    }
}