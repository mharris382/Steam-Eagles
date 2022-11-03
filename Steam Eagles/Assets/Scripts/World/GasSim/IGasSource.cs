using System.Collections.Generic;
using UnityEngine;

namespace GasSim
{
    public interface IGasIO
    {
        IEnumerable<(Vector2Int coord, int amount)> GetSourceCells();
    }
    
    public interface IGasSource : IGasIO
    {
        /// <summary>
        /// notifies potential listeners when gas is taken from the source.  Called as a sum from all cells
        /// called in <see cref="GasSimParticleSystem.DoSources"/> 
        /// </summary>
        /// <param name="amountTaken"></param>
        void GasTakenFromSource(int amountTaken);
        
    }

    public interface IGasSink : IGasSource
    {
        /// <summary>
        /// notifies potential listeners when gas is taken from the source.  Called as a sum from all cells
        /// called in <see cref="GasSimParticleSystem.DoSources"/> 
        /// </summary>
        /// <param name="amountTaken"></param>
        void GasAddedToSink(int amountAdded);
    }
}