using System;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Interactions;
using UnityEngine;
using Zenject;

namespace PhysicsFun.Airships.Installer
{
    public class AirshipControlsInstaller : MonoInstaller<AirshipControlsInstaller>
    {
        [SerializeField] private AirshipFlightConfig flightConfig;
        
        public override void InstallBindings()
        {
            Container.Bind<AirshipFlightConfig>().FromInstance(flightConfig).AsSingle().NonLazy();
            Container.Bind<IHeatPowerToGravityScale>().To<HeatConverter>().AsSingle().NonLazy();
            Container.Bind<IThrusters>().To<ThrusterWrapper>().AsSingle().NonLazy();
        }
    }


    [Serializable]
    public class AirshipFlightConfig : ConfigBase
    {
        
        public float maxGravityScale = 2;
        public AnimationCurve heatPowerToGravityScale = AnimationCurve.Linear(0, 1, 1, 0);
        
        
    }

    class HeatConverter : IHeatPowerToGravityScale
    {
        private readonly AirshipFlightConfig _config;

        public HeatConverter(AirshipFlightConfig config)
        {
            _config = config;
        }

        public float GetGravityScaleAtHeatPower(float heatPower, float height)
        {
            var gscale = _config.heatPowerToGravityScale.Evaluate(heatPower);
            return gscale * _config.maxGravityScale * -1;
        }
    }

    class ThrusterWrapper : IThrusters
    {
        private readonly List<AirshipThruster> _thrusters;

        public ThrusterWrapper(List<AirshipThruster> thrusters)
        {
            _thrusters = thrusters;
        }

        public void SetPower(float power)
        {
            foreach (var airshipThruster in _thrusters)
            {
                airshipThruster.SetPower(power);
            }
        }

        public void SetDirection(Vector2 direction)
        {
            foreach (var airshipThruster in _thrusters)
            {
                airshipThruster.SetDirection(direction);
            }
        }
    }

    public interface IHeatPowerToGravityScale
    {
        float GetGravityScaleAtHeatPower(float heatPower, float height);
    }

    public interface IThrusters
    {
        void SetPower(float power);
        void SetDirection(Vector2 direction);
    }
}