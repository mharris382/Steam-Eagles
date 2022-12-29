using System.Collections;
using GasSim;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzles
{
    public class GasPoweredMachine : MonoBehaviour
    {
        public GameObject powerSource;

        public float consumptionAmount = 0.5f;
        public float consumptionRate = 1f;
    
        private IGasPowerSource _source;
        
        public UnityEvent onMachineStarted;
        public UnityEvent onMachineStopped;
        public UnityEvent<float> onPowerChangedRaw;
        public UnityEvent<float> onPowerChangedNormalized;
    
    
        public FloatReactiveProperty _power = new FloatReactiveProperty(0f);
        public BoolReactiveProperty _isOn = new BoolReactiveProperty(false);
    
        private void Awake()
        {
            _source = GetComponent<IGasPowerSource>();
            _isOn = new BoolReactiveProperty(_source.AvailablePower >= consumptionAmount);
            _power.Select(t => t > consumptionAmount).DistinctUntilChanged().Subscribe(on => _isOn.Value = on);
            _isOn.TakeUntilDestroy(this).Subscribe(t => {
                if (t)
                {
                    onMachineStarted?.Invoke();
                }
                else
                {
                    onMachineStopped?.Invoke();
                }
            });
        
            _power.DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(t => onPowerChangedRaw?.Invoke(t));
            _power.Select(t => t / _source.PowerCapacity).DistinctUntilChanged().TakeUntilDestroy(this).Subscribe(t => onPowerChangedNormalized?.Invoke(t));
        }

        private IEnumerator Start()
        {

            while (enabled)
            {
                yield return new WaitForSeconds(consumptionRate);
                _source.ConsumePower(consumptionAmount);
                _power.Value = _source.AvailablePower;
            }
        }
    }
}