using SaveLoad;

namespace CoreLib.GameTime.SaveData
{
    public class GameTimeNewGameSaveDataCreator : NewGameSaveFileCreator<GameDateTime>
    {
        protected override GameDateTime GetNewGameSaveState()
        {
            return new GameDateTime(0, 0, 6, 30);
        }
    }
}