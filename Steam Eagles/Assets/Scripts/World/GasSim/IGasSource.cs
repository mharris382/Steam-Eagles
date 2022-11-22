using System;
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

    public class GasSimRequestFactory
    {
        private readonly GasSimGrid _gasSimGrid;

        public GasSimRequestFactory(GasSimGrid gasSimGrid)
        {
            this._gasSimGrid = gasSimGrid;
        }
       
        public GasRemovalRequest CreateRemovalRequest(RectInt fixedRect, int fixedAmount,
            Action<RemovedGasMessage> onGasRemoved = null) =>
            CreateRemovalRequest(() => fixedRect, () => fixedAmount, onGasRemoved);

        public GasRemovalRequest CreateRemovalRequest(Func<RectInt> dynamicRect, int fixedAmount,
            Action<RemovedGasMessage> onGasRemoved = null) =>
            CreateRemovalRequest(dynamicRect, () => fixedAmount, onGasRemoved);

        public GasRemovalRequest CreateRemovalRequest(Func<RectInt> dynamicRect, Func<int> dynamicAmount,
            Action<RemovedGasMessage> onGasRemoved = null)
        {
            onGasRemoved ??= (msg) => { };
            return new GasRemovalRequest(dynamicRect, dynamicAmount, onGasRemoved);
        }
        
        public GasRemovalRequest CreateRemovalRequest(RectInt fixedRect, Func<int> dynamicAmount,
            Action<RemovedGasMessage> onGasRemoved = null) =>
            CreateRemovalRequest(() => fixedRect, dynamicAmount, onGasRemoved);
    }
    
    public struct GasRemovalRequest
    {
        private readonly Func<RectInt> targetArea;
        private readonly Func<int> amountToRemove;
        private readonly Action<RemovedGasMessage> onGasRemoved;


        public GasRemovalRequest(Func<RectInt> targetArea, Func<int> amountToRemove, Action<RemovedGasMessage> onGasRemoved)
        {
            this.targetArea = targetArea;
            this.amountToRemove = amountToRemove;
            this.onGasRemoved = onGasRemoved;
        }

        public int GetDesiredRemovalAmount() => amountToRemove();
        
        public IEnumerable<Vector3Int> GetTargetCells() => targetArea().GetCellsWithin();
        
        public void OnGasRemoved(RemovedGasMessage message) => onGasRemoved(message);
    }
    
    public struct GasAdditionRequest
    {
        public Func<RectInt> targetArea;
        public Func<int> amountToAdd;
        public Action<AddedGasMessage> onGasAdded;
        
        public GasAdditionRequest(Vector2Int targetCell, int amountToAdd, Action<AddedGasMessage> onGasAdded)
        {
            this.targetArea = () => new RectInt(targetCell, Vector2Int.one);
            this.amountToAdd = () => amountToAdd;
            this.onGasAdded = onGasAdded;
        }
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

