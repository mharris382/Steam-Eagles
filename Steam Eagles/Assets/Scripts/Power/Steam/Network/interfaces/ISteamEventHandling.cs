using System;
using UnityEngine;

namespace Power.Steam.Network
{
    public interface ISteamEventHandling
    {
        IObservable<GasConsumedEventData> GasConsumedObservable { get; }
        IObservable<GasProducedEventData> GasProducedObservable { get; }
    }
    
    
    public struct GasConsumedEventData
    {
        public Vector2Int Position { get; }
        public float Amount { get; }

        public GasConsumedEventData(Vector2Int position, float amount)
        {
            Position = position;
            Amount = amount;
        }
    }

    public struct GasProducedEventData
    {
        public Vector2Int Position { get; }
        public float Amount { get; }

        public GasProducedEventData(Vector2Int position, float amount)
        {
            Position = position;
            Amount = amount;
        }
    }
}