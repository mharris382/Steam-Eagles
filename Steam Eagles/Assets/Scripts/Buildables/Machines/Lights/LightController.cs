using System;
using Buildings;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables.Lights
{
    [RequireComponent(typeof(BuildableMachineBase))]
    public class LightController : MonoBehaviour, IElectricityConsumer
    {
        [SerializeField] BoolReactiveProperty powered = new BoolReactiveProperty(false);
        [SerializeField] FloatReactiveProperty consumptionRate = new FloatReactiveProperty(5);
        [Required] public GameObject light;

        private ElectricityConsumers _consumers;
        private BuildableMachineBase _machineBase;
        public BuildableMachineBase MachineBase => _machineBase ? _machineBase : _machineBase = GetComponent<BuildableMachineBase>();

        public RectInt GridRect => new RectInt(!MachineBase.IsFlipped ? MachineBase.CellPosition : MachineBase.CellPosition - new Vector2Int(MachineBase.MachineGridSize.x, 0), MachineBase.MachineGridSize);
        
        
        public bool Powered
        {
            get => powered.Value;
            set => powered.Value = value;
        }
        
        public IReadOnlyReactiveProperty<float> ConsumptionRateProperty => consumptionRate;


       [Inject] void Install(ElectricityConsumers consumers)
       {
           _consumers = consumers;
           if (enabled) _consumers.Register(this);
       }

       private void Start() => powered.StartWith(Powered).Subscribe(light.SetActive).AddTo(this);
       private void OnEnable() => _consumers?.Register(this);
       private void OnDisable() => _consumers?.Unregister(this);
    }
}