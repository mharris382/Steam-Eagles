using UnityEngine;

namespace WorldV2.FluidSimulation
{
    public interface IFluidContainer
    {
        
    }
    public abstract class FluidContainerBase : MonoBehaviour
    {
        public abstract Vector2 CellSize { get; }
    }
}
