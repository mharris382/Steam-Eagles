using UnityEngine;

namespace Damage.Core
{
    /// <summary>
    /// interface for anything that wants to be affected by a storm
    /// </summary>
    public interface IStormSubject
    {
        Rigidbody2D Rigidbody2D { get; }
    }
}