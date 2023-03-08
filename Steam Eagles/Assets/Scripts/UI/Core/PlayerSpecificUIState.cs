using Players;

namespace UI.Core
{
    public abstract class PlayerSpecificUIState : UIState
    {
        public Player Player { get; }

        protected PlayerSpecificUIState(Player player)
        {
            Player = player;
        }
    }
}