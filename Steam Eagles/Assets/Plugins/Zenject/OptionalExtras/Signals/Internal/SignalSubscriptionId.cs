using System;
using System.Diagnostics;

namespace Zenject
{
    [DebuggerStepThrough]
    public struct SignalSubscriptionId : IEquatable<SignalSubscriptionId>
    {
        private BindingId _signalId;

        public SignalSubscriptionId(BindingId signalId, object callback)
        {
            _signalId = signalId;
            Callback = callback;
        }

        public BindingId SignalId => _signalId;

        public object Callback { get; }

        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 29 + _signalId.GetHashCode();
                hash = hash * 29 + Callback.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object that)
        {
            if (that is SignalSubscriptionId) return Equals((SignalSubscriptionId)that);

            return false;
        }

        public bool Equals(SignalSubscriptionId that)
        {
            return Equals(_signalId, that._signalId)
                   && Equals(Callback, that.Callback);
        }

        public static bool operator ==(SignalSubscriptionId left, SignalSubscriptionId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SignalSubscriptionId left, SignalSubscriptionId right)
        {
            return !left.Equals(right);
        }
    }
}