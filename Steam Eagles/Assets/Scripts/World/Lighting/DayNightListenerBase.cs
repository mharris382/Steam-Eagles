using CoreLib.GameTime;
using UnityEngine;
using Zenject;

namespace GasSim.Lighting
{
    public abstract class DayNightListenerBase : MonoBehaviour, IDayNightListener
    {
        private DayNightListeners _listeners;

        public abstract void OnTimeChanged(GameTime gameTime);


        [Inject] void Install(DayNightListeners listeners)
        {
            _listeners = listeners;   
        }

        private void OnEnable()
        {
            if (_listeners != null) _listeners.Register(this);
        }

        private void OnDisable()
        {
            if (_listeners != null) _listeners.Unregister(this);
        }

        public virtual void OnDayStarted() { }

        public virtual void OnNightStarted() { }
    }
}