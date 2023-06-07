using System;

namespace Zenject
{
    // Note that there's a reason we don't just have a generic
    // argument for signal type - because when using struct type signals it throws
    // exceptions on AOT platforms
    public class SignalCallbackWrapper : IDisposable
    {
        private readonly Action<object> _action;
        private readonly object _identifier;
        private readonly SignalBus _signalBus;
        private readonly Type _signalType;

        public SignalCallbackWrapper(
            SignalBindingBindInfo bindInfo,
            Action<object> action,
            SignalBus signalBus)
        {
            _signalType = bindInfo.SignalType;
            _identifier = bindInfo.Identifier;
            _signalBus = signalBus;
            _action = action;

            signalBus.SubscribeId(bindInfo.SignalType, _identifier, OnSignalFired);
        }

        public void Dispose()
        {
            _signalBus.UnsubscribeId(_signalType, _identifier, OnSignalFired);
        }

        private void OnSignalFired(object signal)
        {
            _action(signal);
        }
    }
}