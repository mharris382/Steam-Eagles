using System;
using System.Collections.Generic;
using ModestTree;
#if ZEN_SIGNALS_ADD_UNIRX
using UniRx;
#endif

namespace Zenject
{
    public class SignalDeclaration : ITickable, IDisposable
    {
        private readonly List<object> _asyncQueue = new();
        private readonly SignalMissingHandlerResponses _missingHandlerResponses;
        private readonly ZenjectSettings.SignalSettings _settings;

#if ZEN_SIGNALS_ADD_UNIRX
        readonly Subject<object> _stream = new Subject<object>();
#endif

        public SignalDeclaration(
            SignalDeclarationBindInfo bindInfo,
            [InjectOptional] ZenjectSettings zenjectSettings)
        {
            zenjectSettings = zenjectSettings ?? ZenjectSettings.Default;
            _settings = zenjectSettings.Signals ?? ZenjectSettings.SignalSettings.Default;

            BindingId = new BindingId(bindInfo.SignalType, bindInfo.Identifier);
            _missingHandlerResponses = bindInfo.MissingHandlerResponse;
            IsAsync = bindInfo.RunAsync;
            TickPriority = bindInfo.TickPriority;
        }

#if ZEN_SIGNALS_ADD_UNIRX
        public IObservable<object> Stream
        {
            get { return _stream; }
        }
#endif

        public List<SignalSubscription> Subscriptions { get; } = new();

        public int TickPriority { get; }

        public bool IsAsync { get; }

        public BindingId BindingId { get; }

        public void Dispose()
        {
            if (_settings.RequireStrictUnsubscribe)
                Assert.That(Subscriptions.IsEmpty(),
                    "Found {0} signal handlers still added to declaration {1}", Subscriptions.Count, BindingId);
            else
                // We can't rely entirely on the destruction order in Unity because of
                // the fact that OnDestroy is completely unpredictable.
                // So if you have a GameObjectContext at the root level in your scene, then it
                // might be destroyed AFTER the SceneContext.  So if you have some signal declarations
                // in the scene context, they might get disposed before some of the subscriptions
                // so in this case you need to disconnect from the subscription so that it doesn't
                // try to remove itself after the declaration has been destroyed
                for (var i = 0; i < Subscriptions.Count; i++)
                    Subscriptions[i].OnDeclarationDespawned();
        }

        public void Fire(object signal)
        {
            Assert.That(signal.GetType().DerivesFromOrEqual(BindingId.Type));

            if (IsAsync)
                _asyncQueue.Add(signal);
            else
                // Cache the callback list to allow handlers to be added from within callbacks
                using (var block = DisposeBlock.Spawn())
                {
                    var subscriptions = block.SpawnList<SignalSubscription>();
                    subscriptions.AddRange(Subscriptions);
                    FireInternal(subscriptions, signal);
                }
        }

        private void FireInternal(List<SignalSubscription> subscriptions, object signal)
        {
            if (subscriptions.IsEmpty()
#if ZEN_SIGNALS_ADD_UNIRX
                && !_stream.HasObservers
#endif
               )
            {
                if (_missingHandlerResponses == SignalMissingHandlerResponses.Warn)
                    Log.Warn(
                        "Fired signal '{0}' but no subscriptions found!  If this is intentional then either add OptionalSubscriber() to the binding or change the default in ZenjectSettings",
                        signal.GetType());
                else if (_missingHandlerResponses == SignalMissingHandlerResponses.Throw)
                    throw Assert.CreateException(
                        "Fired signal '{0}' but no subscriptions found!  If this is intentional then either add OptionalSubscriber() to the binding or change the default in ZenjectSettings",
                        signal.GetType());
            }

            for (var i = 0; i < subscriptions.Count; i++)
            {
                var subscription = subscriptions[i];

                // This is a weird check for the very rare case where an Unsubscribe is called
                // from within the same callback (see TestSignalsAdvanced.TestSubscribeUnsubscribeInsideHandler)
                if (Subscriptions.Contains(subscription)) subscription.Invoke(signal);
            }

#if ZEN_SIGNALS_ADD_UNIRX
            _stream.OnNext(signal);
#endif
        }

        public void Tick()
        {
            Assert.That(IsAsync);

            if (!_asyncQueue.IsEmpty())
                // Cache the callback list to allow handlers to be added from within callbacks
                using (var block = DisposeBlock.Spawn())
                {
                    var subscriptions = block.SpawnList<SignalSubscription>();
                    subscriptions.AddRange(Subscriptions);

                    // Cache the signals so that if the signal is fired again inside the handler that it
                    // is not executed until next frame
                    var signals = block.SpawnList<object>();
                    signals.AddRange(_asyncQueue);

                    _asyncQueue.Clear();

                    for (var i = 0; i < signals.Count; i++) FireInternal(subscriptions, signals[i]);
                }
        }

        public void Add(SignalSubscription subscription)
        {
            Assert.That(!Subscriptions.Contains(subscription));
            Subscriptions.Add(subscription);
        }

        public void Remove(SignalSubscription subscription)
        {
            Subscriptions.RemoveWithConfirm(subscription);
        }

        public class Factory : PlaceholderFactory<SignalDeclarationBindInfo, SignalDeclaration>
        {
        }
    }
}