using System;
using Sirenix.OdinInspector;
using UniRx;

namespace CoreLib
{
    [Serializable]
    public class GlobalElectricitySettings
    {
        [OnValueChanged(nameof(SetPowerRequirement))]
        public bool powerRequirementsEnabled = true;



        private Subject<bool> _onPowerRequirementChanged = new();
        public IObservable<bool> OnPowerRequirementChanged => _onPowerRequirementChanged;



        void SetPowerRequirement(bool enabled)
        {
            // if(enabled == powerRequirementsEnabled)
            //     return;
            powerRequirementsEnabled = enabled;
            _onPowerRequirementChanged.OnNext(enabled);
        }
    }
}