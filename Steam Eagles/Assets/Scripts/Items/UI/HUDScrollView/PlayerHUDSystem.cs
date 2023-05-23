using Players.PCController;
using UnityEngine;
using Zenject;

namespace Items.UI.HUDScrollView
{
    public class PlayerHUDSystem : PCSystem, IInitializable, ITickable, ILateTickable
    {
        private PlayerHUD _hudViewInstance;
        private PlayerHUDInstance _hudInstance;
        private DiContainer _container;
        private PlayerHUDPrefab _hudPrefab;

        public class Factory : PlaceholderFactory<PC, PlayerHUDSystem>, ISystemFactory<PlayerHUDSystem> { }
        
        public PlayerHUDSystem(PC pc, PlayerHUDPrefab hudPrefab, DiContainer container) : base(pc)
        {
            _container = container;
            _hudPrefab = hudPrefab;
        }

        public void Initialize()
        {
            Debug.Log($"Initialized HUD for player {Pc.PlayerNumber}");
            _hudViewInstance = _container.InstantiatePrefab(_hudPrefab.Prefab).GetComponent<PlayerHUD>();
            _hudInstance = new PlayerHUDInstance(_hudViewInstance);
            _hudInstance.SetPlayer(this.Pc.PlayerNumber);
        }

        public void Tick()
        {
            
        }

        public void LateTick()
        {
            
        }
    }
}