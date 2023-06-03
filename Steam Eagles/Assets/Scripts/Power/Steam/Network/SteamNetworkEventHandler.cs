
using System;
using UniRx;

namespace Power.Steam.Network
{
    public class SteamNetworkEventHandler : ISteamEventHandling
    {
        Subject<GasConsumedEventData> _gasConsumedSubject = new Subject<GasConsumedEventData>();
        Subject<GasProducedEventData> _gasProducedSubject = new Subject<GasProducedEventData>();
        public IObservable<GasConsumedEventData> GasConsumedObservable => _gasConsumedSubject;
        public IObservable<GasProducedEventData> GasProducedObservable => _gasProducedSubject;
    }
}