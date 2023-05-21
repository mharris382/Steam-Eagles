using Players.PCController;
using Zenject;

namespace Items.UI.HUDScrollView
{
    public class PlayerHUDSystem : PCSystem, IInitializable, ITickable, ILateTickable
    {
        private readonly PlayerHUD _hudViewInstance;

        public class Factory : PlaceholderFactory<PC, PlayerHUDSystem> , ISystemFactory<PlayerHUDSystem> {}
        public PlayerHUDSystem(PC pc, PlayerHUD hudViewInstance) : base(pc)
        {
            _hudViewInstance = hudViewInstance;
        }

        public void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public void LateTick()
        {
            throw new System.NotImplementedException();
        }
    }
}