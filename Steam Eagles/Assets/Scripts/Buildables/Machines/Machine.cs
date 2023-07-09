using UnityEngine;
using Zenject;

namespace Buildables
{
    public abstract class Machine<T> : MonoBehaviour where T : Machine<T>
    {
        private MachineHandler<T> _handlers;

        [Inject] void Install(MachineHandler<T> handlers)
        {
            if (enabled) handlers.Register(this as T);
            _handlers = handlers;
        }

        private void OnEnable()
        {
            if (_handlers != null) _handlers.Register(this as T);
        }

        private void OnDisable()
        {
            if (_handlers != null) _handlers.Unregister(this as T);
        }
    }
}