using UniRx;
using UnityEngine;

namespace GasSim
{
    public class AutoRegisterGasIO : MonoBehaviour
    {
        private void OnEnable()
        {
            ToggleSinks();
            ToggleSources();
        }

        private void OnDisable()
        {
            ToggleSinks();
            ToggleSources();
        }

        private void ToggleSinks()
        {
            var sinks = GetComponents<IGasSink>();
            if (sinks.Length > 0)
            {
                foreach (var gasSink in sinks)
                {
                    MessageBroker.Default.Publish<IGasSink>(gasSink);
                }
            }
        }

        private void ToggleSources()
        {
            var sources = GetComponents<IGasSource>();
            if (sources.Length > 0)
            {
                foreach (var gasSource in sources)
                {
                    MessageBroker.Default.Publish<IGasSource>(gasSource);
                }
            }
        }
    }
}