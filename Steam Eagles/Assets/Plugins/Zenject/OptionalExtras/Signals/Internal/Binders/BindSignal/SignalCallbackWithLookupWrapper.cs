using System;

namespace Zenject
{
    // Note that there's a reason we don't just have a generic
    // argument for signal type - because when using struct type signals it throws
    // exceptions on AOT platforms
    public class SignalCallbackWithLookupWrapper : IDisposable
    {
        private readonly DiContainer _container;
        private readonly object _identifier;
        private readonly Guid _lookupId;
        private readonly Func<object, Action<object>> _methodGetter;
        private readonly Type _objectType;
        private readonly SignalBus _signalBus;
        private readonly Type _signalType;

        public SignalCallbackWithLookupWrapper(
            SignalBindingBindInfo signalBindInfo,
            Type objectType,
            Guid lookupId,
            Func<object, Action<object>> methodGetter,
            SignalBus signalBus,
            DiContainer container)
        {
            _objectType = objectType;
            _signalType = signalBindInfo.SignalType;
            _identifier = signalBindInfo.Identifier;
            _container = container;
            _methodGetter = methodGetter;
            _signalBus = signalBus;
            _lookupId = lookupId;

            signalBus.SubscribeId(signalBindInfo.SignalType, _identifier, OnSignalFired);
        }

        public void Dispose()
        {
            _signalBus.UnsubscribeId(_signalType, _identifier, OnSignalFired);
        }

        private void OnSignalFired(object signal)
        {
            var objects = _container.ResolveIdAll(_objectType, _lookupId);

            for (var i = 0; i < objects.Count; i++) _methodGetter(objects[i])(signal);
        }
    }
}