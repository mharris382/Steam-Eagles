namespace SaveLoad
{
    /// <summary>
    /// used to create a new save file for an existing game by reading the save state from the game world
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ExistingGameSaveFileCreator<T>
    {
        protected abstract T GetSaveState();
    }
}