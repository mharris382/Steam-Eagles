namespace SaveLoad.CoreSave
{
    /// <summary>
    /// used implicitly by the SaveLoadManager to create a new save file when none exists
    /// </summary>
    public class CoreSaveDataNewGameCreator : NewGameSaveFileCreator<CoreSaveData>
    {
        protected override CoreSaveData GetNewGameSaveState()
        {
            return new CoreSaveData(1, "Builder", "Transporter");
        }
    }
}