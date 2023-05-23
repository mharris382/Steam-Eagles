namespace Items.UI.HUDScrollView
{
    public class PlayerHUDPrefab
    {
        public PlayerHUDPrefab(PlayerHUD prefab)
        {
            this.Prefab = prefab;
        }

        public PlayerHUD Prefab { get;  }
    }
}