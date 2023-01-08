using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace GasSim
{
    public class GasSourceConsumer : GasSource, IGasConsumer
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
            onGasEvent.AddListener(OnGasRemovedFromTank);
            
            //cannot have a gas source and consumer enabled at the same time
            if (gameObject.TryGetComponent<GasSinkSupplier>(out var sinkSupplier))
            {
                sinkSupplier.enabled = false;
            }
        }
       
        public int GetRequestedSupply()
        {
            return enabled ? Mathf.Min(unclaimedSupply, GetSupplyAmount()) : 0;
        }

        public override IEnumerable<(Vector2Int coord, int amount)> GetSourceCells()
        {
            return base.GetSourceCells().Where(t => unclaimedSupply > t.amount);
        }

        
        /// <summary>
        /// called in order to pass gas into the simulation from the tank
        /// </summary>
        /// <param name="amount">amount of gas added</param>
        public void ReceiveSupply(int amount)
        {
            unclaimedSupply += amount;
        }

        
        /// <summary>
        /// called when the simulation removes gas from the tank
        /// </summary>
        /// <param name="amount">amount of gas removed</param>
        void OnGasRemovedFromTank(int amount)
        {
            unclaimedSupply -= amount;
        }
    }
}