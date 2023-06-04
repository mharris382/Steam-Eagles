using System;
using System.ComponentModel;
using Power.Steam.Network;
using UniRx;
using UnityEngine;
using Zenject;

namespace Power
{
    public static class SteamIO
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.Bind<SteamProducers>().AsSingle().NonLazy();
                Container.Bind<SteamConsumers>().AsSingle().NonLazy();
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Producer, Producer.Factory>().AsSingle().NonLazy();
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Consumer, Consumer.Factory>().AsSingle().NonLazy();
            }
        }
        public class Consumer : ISteamConsumer, IDisposable
        {
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>,  Consumer> { }

            private readonly Vector2Int _cell;

            private readonly IDisposable _disposable;

            private readonly Func<float> _consumptionRateGetter;

            private readonly Action<float> _onSteamConsumed;

            public Consumer(Vector2Int cell, Func<float> consumptionRateGetter, Action<float> onSteamConsumed, SteamConsumers consumers)
            {
                _cell = cell;
                _consumptionRateGetter = consumptionRateGetter;
                _onSteamConsumed = onSteamConsumed;
                consumers.AddSystem(cell, this);
                _disposable = Disposable.Create(() => consumers.RemoveSystem(cell));
            }

            public bool IsActive { get; set; }

            public float GetSteamConsumptionRate() => _consumptionRateGetter();

            public void ConsumeSteam(float amount) => _onSteamConsumed(amount);

            public void Dispose() => _disposable.Dispose();
        }
        public class Producer : ISteamProducer, IDisposable
        {
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>, Producer> { }

            private readonly Vector2Int _cell;
            private readonly Func<float> _productionRateGetter;
            private readonly Action<float> _onSteamProduced;
            private readonly IDisposable _disposable;

            public Producer(Vector2Int cell,  Func<float> productionRateGetter,Action<float> onSteamProduced, SteamProducers producers)
            {
                _cell = cell;
                _productionRateGetter = productionRateGetter;
                _onSteamProduced = onSteamProduced;
                producers.AddSystem(cell, this);
                _disposable = Disposable.Create(() => producers.RemoveSystem(cell));
            }
            public bool IsActive { get; set; }

            public float GetSteamProductionRate() => _productionRateGetter();
            public void ProduceSteam(float amount) => _onSteamProduced(amount);

            public void Dispose() => _disposable?.Dispose();
        }
    }
}