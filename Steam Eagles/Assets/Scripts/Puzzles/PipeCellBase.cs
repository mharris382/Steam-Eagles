using UnityEngine;

namespace Puzzles
{
    public abstract class PipeCellBase : CellHelper
    {
        
    }
    
    public enum PipeConnectionState
    {
        NOT_CONNECTED = 0,
        IN_CONNECTED = 1 << 0,
        OUT_CONNECTED = 1 << 1,
        FULLY_CONNECTED = IN_CONNECTED | OUT_CONNECTED
    }
}