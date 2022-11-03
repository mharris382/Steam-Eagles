using GasSim;
using UnityEngine;

namespace GasSim
{
    [AddComponentMenu("SteamEagles/Gas/GasSink")]
    public class GasSink : GasSource, IGasSink
    {
        public void GasAddedToSink(int amountAdded)
        {
            onGasEvent?.Invoke(amountAdded);
        }
    }
}