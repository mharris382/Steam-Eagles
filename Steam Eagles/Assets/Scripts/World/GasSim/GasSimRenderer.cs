using UnityEngine;

namespace GasSim
{
    public abstract class GasSimRenderer : MonoBehaviour, IGasSimRenderer
    {
        GasSimResources _resources;
        public void AssignResources(GasSimResources resources)
        {
            _resources = resources;
        }

        public bool IsValid()
        {
            return _resources != null;
        }
    }
}