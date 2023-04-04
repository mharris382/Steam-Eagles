namespace SaveLoad
{
    public interface INewGameSaveFileCreator
    {
        void CreateNewSaveFile(string savePath);
    }
    
    public interface IGameSaveFileCreator
    {
        void SaveGame(string savePath);
    }
    
    public interface IGameDataSaver
    {
        void SaveGame(string savePath,bool debug=false);
    }
}