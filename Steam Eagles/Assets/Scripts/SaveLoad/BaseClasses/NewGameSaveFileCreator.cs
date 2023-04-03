using UnityEngine;

namespace SaveLoad
{
    public abstract class SaveFileCreator<T> : IGameDataSaver
    {
        private string GetSaveFileName() => typeof(T).Name;
        protected string GetSaveFilePath(string savePath) => $"{savePath}/{GetSaveFileName()}.json";
        protected abstract T GetSaveState();
        public void SaveGame(string savePath, bool debug=false)
        {
            var data = GetSaveState();
            if (debug) Debug.Log($"Saving {data} to {savePath}");
            SaveData(data, GetSaveFilePath(savePath));
        }
        private void SaveData(T data, string filePath)
        {
            var json = JsonUtility.ToJson(data);
            System.IO.File.WriteAllText(filePath, json);
        }
    }
    /// <summary>
    /// implement this for each type of save data object so that it can be used to
    /// initialize the save state of a new game.  After that the operation for
    /// starting a new game and loading an existing game will be identical
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class NewGameSaveFileCreator<T> : SaveFileCreator<T>, INewGameSaveFileCreator
    {
        private string GetSaveFileName() => typeof(T).Name;

        protected abstract T GetNewGameSaveState();

        protected override T GetSaveState()
        {
            return GetNewGameSaveState();
        }

        public void CreateNewSaveFile(string savePath) => SaveGame(savePath);
    }
}