using UnityEngine;

namespace GasSim
{
    public interface IGasSim
    {
        enum GridResolution
        {
            FULL,
            HALF,
            QUART,
            EIGHTH,
            X16,
            X32
        }

    
        Grid Grid { get; }
    
        RectInt SimulationRect {get;}

        void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter);

        bool TryAddGasToCell(Vector2Int coord, int amount);
    }
}