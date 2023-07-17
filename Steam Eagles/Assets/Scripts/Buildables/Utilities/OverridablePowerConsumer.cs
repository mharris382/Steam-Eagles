using System;

namespace Buildings
{
    public class OverridablePowerConsumer : CustomPowerConsumer
    {
        public float supplyRate = 1;


        private bool _assigned;
        private Func<float> _consumeRate;
        private Action<float> _consume;

        public void SetOverride(Func<float> consumeRate, Action<float> consume)
        {
            _assigned = true;
            _consumeRate = consumeRate;
            _consume = consume;
        }
        
        
        protected override float GetConsumptionRate()
        {
            return _assigned ? _consumeRate() : supplyRate;
        }

        protected override void Consume(float amount)
        {
            if (_assigned) _consume(amount);
        }
    }
}