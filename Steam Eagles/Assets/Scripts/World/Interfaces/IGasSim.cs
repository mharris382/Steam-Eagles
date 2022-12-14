using UnityEngine;

namespace GasSim
{
    public enum GridResolution
    {
        FULL,
        HALF,
        QUART,
        EIGHTH,
        X16,
        X32
    }
    public interface IGasSim
    {
        
        
    
        Grid Grid { get; }
    
        RectInt SimulationRect {get;}

        void SetStateOfMatter(Vector2Int coord, StateOfMatter stateOfMatter);

        bool TryAddGasToCell(Vector2Int coord, int amount);
        bool TryRemoveGasFromCell(Vector2Int coord, int amount);
        bool CanAddGasToCell(Vector2Int coord, ref int amount);
        bool CanRemoveGasFromCell(Vector2Int coord, ref int amount);        
        
    }
}