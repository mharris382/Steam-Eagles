using System;
using UnityEngine;
using Zenject;

namespace Power
{
    public interface ISteamConsumer
    {
        bool IsActive { get; }
        float GetSteamConsumptionRate();
        void ConsumeSteam(float amount);
    }

    public interface ISteamProducer
    {
        bool IsActive { get; set; }
        float GetSteamProductionRate();
        void ProduceSteam(float amount);
    }

    public static class SteamIO
    {
        public class Installer : Installer<Installer>
        {
            public override void InstallBindings()
            {
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Producer, Producer.Factory>().AsSingle().NonLazy();
                Container.BindFactory<Vector2Int, Func<float>, Action<float>, Consumer, Consumer.Factory>().AsSingle().NonLazy();
            }
        }
        public class Consumer : ISteamConsumer
        {
            public Consumer(Vector2Int cell, Func<float> consumptionRateGetter, Action<float> onSteamConsumed)
            {
                _consumptionRateGetter = consumptionRateGetter;
                _onSteamConsumed = onSteamConsumed;
            }

            public bool IsActive { get; set; }
            private readonly Func<float> _consumptionRateGetter;
            private readonly Action<float> _onSteamConsumed;
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>,  Consumer> { }
            public float GetSteamConsumptionRate()
            {
                throw new NotImplementedException();
            }

            public void ConsumeSteam(float amount)
            {
                throw new NotImplementedException();
            }
        }
        public class Producer : ISteamProducer
        {
            public class Factory : PlaceholderFactory<Vector2Int, Func<float>, Action<float>, Producer> {
            }
            private readonly Func<float> _productionRateGetter;
            private readonly Action<float> _onSteamProduced;
            public Producer(Vector2Int cell,  Func<float> productionRateGetter,Action<float> onSteamProduced)
            {
                _productionRateGetter = productionRateGetter;
                _onSteamProduced = onSteamProduced;
            }
            public bool IsActive
            {
                get;
                set;
            }
            public float GetSteamProductionRate()
            {
                throw new System.NotImplementedException();
            }

            public void ProduceSteam(float amount)
            {
                throw new System.NotImplementedException();
            }
        }
    }

   
}