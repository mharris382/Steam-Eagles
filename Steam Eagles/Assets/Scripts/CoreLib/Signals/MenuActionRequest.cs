namespace CoreLib
{
    /// <summary>
    /// represents an request to open, close, or change menu states from a player
    /// </summary>
    public struct MenuActionRequest
    {
        public readonly int playerNumber;
        public readonly MenuAction menuAction;
        public MenuActionRequest(int playerNumber, MenuAction menuAction)
        {
            this.playerNumber = playerNumber;
            this.menuAction = menuAction;
        }
    }

    public enum MenuAction
    {
        OPEN_MENU,
        CLOSE_MENU,
    }
}