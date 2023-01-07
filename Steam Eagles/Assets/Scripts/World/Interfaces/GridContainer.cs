using UnityEngine;

namespace GasSim
{
    public interface IGridContainer
    {
        public Transform transform { get; }
        public Grid Grid { get; }
        public BoundsInt Bounds { get; }
    }
}