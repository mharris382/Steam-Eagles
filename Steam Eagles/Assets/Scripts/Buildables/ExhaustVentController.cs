using System;
using Power;
using UniRx;
using Zenject;

namespace Buildables
{
    public class ExhaustVentController : IDisposable
    {
        private readonly ExhaustVent _exhaustVent;
        private readonly HypergasEngineConfig _config;

        public class Factory : PlaceholderFactory<ExhaustVent, ExhaustVentController> { }
        
        Subject<float> _onSteamConsumed;
        public ExhaustVentController(ExhaustVent exhaustVent, HypergasEngineConfig config, SteamIO.Consumer.Factory consumerFactory)
        {
            _exhaustVent = exhaustVent;
            _config = config;
            var cellPosition = exhaustVent.cell.BuildingSpacePosition;
            var consumer = consumerFactory.Create(cellPosition, GetConsumptionRate, ConsumeSteam);
            _onSteamConsumed = new Subject<float>();
            IObservable<float> onConsumed = _onSteamConsumed.Where(t => t >= 0.01f);
            _onSteamConsumed.Subscribe(t => exhaustVent.ConsumptionAmountTotal += t);
            onConsumed.Subscribe(t => exhaustVent.ConsumptionCount++);
            onConsumed.Subscribe(t => exhaustVent.onSteamConsumed.Invoke(t));
            consumer.IsActive = true;
        }
        float GetConsumptionRate() => _config.exhaustVentMaxConsumerRate;

        void ConsumeSteam(float amount)
        {
            if (amount >= 0.01f)
            {
                _onSteamConsumed.OnNext(amount);
            }
        }

        public void Dispose()
        {
            _onSteamConsumed?.Dispose();
        }
    }
}