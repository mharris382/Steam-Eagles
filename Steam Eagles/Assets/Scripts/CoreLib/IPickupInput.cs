using UniRx;

namespace CoreLib
{
    public interface IPickupInput
    {
        public bool WantsToPickup(string itemID);

        ReadOnlyReactiveProperty<bool> IsPickingUp { get; }
    }
}