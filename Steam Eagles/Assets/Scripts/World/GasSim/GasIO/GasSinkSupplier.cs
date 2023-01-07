using System;

namespace GasSim
{
    /// <summary>
    /// sink which removes gas from the simulation and passes it into the pipe network
    /// </summary>
    public class GasSinkSupplier : GasSink, IGasSupplier
    {
        private int _unclaimedSupply = 0;
        public int maximumStoredSupply = 1000;
        public int unclaimedSupply
        {
            get => _unclaimedSupply;
            set
            {
                _unclaimedSupply = value;
                if (_unclaimedSupply > maximumStoredSupply) _unclaimedSupply = maximumStoredSupply;
            }
        }
        
        private void OnEnable()
        {
            onGasEvent.AddListener(OnGasRemovedFromSimulation);
        }

        public int GetUnclaimedSupply()
        {
            return enabled ? unclaimedSupply : 0;
        }

        /// <summary>
        /// called by the pipe network to get gas from the supplier
        /// </summary>
        /// <param name="amount"></param>
        public void ClaimSupply(int amount)
        {
            _unclaimedSupply -= amount;
        }
        
        /// <summary>
        /// called by the simulation when gas is removed from the simulation
        /// </summary>
        /// <param name="amount"></param>
        void OnGasRemovedFromSimulation(int amount)
        {
            _unclaimedSupply += amount;
        }
    }
}