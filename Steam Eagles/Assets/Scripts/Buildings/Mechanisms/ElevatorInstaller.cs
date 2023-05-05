using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UnityEngine;
using Zenject;

namespace Buildings.Mechanisms
{
    [RequireComponent(typeof(ElevatorMechanism))]
    public class ElevatorInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(ElevatorMechanism)).FromComponentOn(gameObject).AsSingle()
                .NonLazy();
            Container.BindInterfacesAndSelfTo<ElevatorStopEvaluator>().FromNew().AsSingle().NonLazy();
            Container.Bind<IElevatorMechanism>().To<ElevatorMover>().AsSingle().NonLazy();
            Container.BindFactory<int, ElevatorOption, ElevatorOption.Factory>();
            Container.Bind<IActionOptionsProvider>().To<ElevatorOptionsProvider>().AsSingle();
        }
    }

    public class ElevatorOptionsProvider : IActionOptionsProvider
    {
        private ElevatorOption[] _elevatorOptions;
        
        public ElevatorOptionsProvider(ElevatorMechanism elevatorMechanism, ElevatorOption.Factory factory)
        {
            var stops = elevatorMechanism.GetStops().ToArray();
            _elevatorOptions = new ElevatorOption[stops.Length];
            for (int i = 0; i < stops.Length; i++) _elevatorOptions[i] = factory.Create(i);
        }

        public IEnumerable<IActionOption> GetOptions()
        {
            foreach (var option in _elevatorOptions)
            {
                yield return option;
            }
        }
    }

    public class ElevatorOption : IActionOption
    {
        private readonly int _floor;
        private readonly IElevatorMechanism _movementMechanism;
        private readonly string _label;
        private readonly ElevatorMechanism _elevator;

        public ElevatorOption(int floor, IElevatorMechanism movementMechanism, ElevatorMechanism elevator)
        {
            _floor = floor;
            _movementMechanism = movementMechanism;
            _elevator = elevator;
            var stop = _elevator.GetStop(floor);
            _label = stop.name;
        }
        public class Factory : PlaceholderFactory<int, ElevatorOption>{}

        public string OptionName => _label;
        public bool IsAvailable { get; }
        public void Execute()
        {
            _movementMechanism.MoveToFloor(_floor);
        }
    }
}