using System;

namespace Buildings
{
    public class OverridablePowerSupplier : CustomPowerSupplier
    {
        public float defaultRate = 1;


        private bool _assigned;
        private Func<float> _supplyRate;
        private Func<float, float> _removeSupply;

        public void SetOverride(Func<float> supplyRate, Func<float, float> removeSupply)
        {
            _assigned = true;
            _supplyRate = supplyRate;
            _removeSupply = removeSupply;
        }
        
        protected override float GetSupplyRate()
        {
            return _assigned ? _supplyRate() : defaultRate;
        }

        protected override float RemoveSupply(float amount)
        {
            return _assigned ? _removeSupply(amount) : amount;
        }
    }
}