using System;
using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    public interface IGasSource : IGasIO
    {
        /// <summary>
        /// notifies potential listeners when gas is taken from the source.  Called as a sum from all cells
        /// called in <see cref="GasSimParticleSystem.DoSources"/> 
        /// </summary>
        /// <param name="amountTaken"></param>
        void GasTakenFromSource(int amountTaken);
        
    }


    /// <summary>
    /// message that is sent when gas is successfully injected into the simulation
    /// </summary>
    public struct AddedGasMessage
    {
        public readonly Vector3Int cellCoordinate;
        public readonly Vector3 cellPositionWS;
        public readonly int amountAdded;
    }
    
    
    /// <summary>
    /// message that is sent when gas is successfully removed from the simulation
    /// </summary>
    public struct RemovedGasMessage
    {
        public readonly Vector3Int cellCoordinate;
        public readonly Vector3 cellPositionWS;
        public readonly int amountRemoved;
    }

    
    public static class RectExtensions
    {
        public static IEnumerable<Vector3Int> GetCellsWithin(this RectInt rect)
        {
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                for (int y = rect.yMin; y < rect.yMax; y++)
                {
                    yield return new Vector3Int(x, y, 0);
                }
            }
        }
    }
}

