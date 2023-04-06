using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveLoad
{
    public class GameSaveFileCreator
    {
        private readonly List<IGameSaveFileCreator> saveHandlers;

        public GameSaveFileCreator()
        {
            this.saveHandlers = ReflectionUtils.GetConcreteInstances<IGameSaveFileCreator>();
        }

        public void SaveGame(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log($"Saving Game at path:\n{savePath}" );
                Directory.CreateDirectory(savePath);
            }
            foreach (var saveFileCreator in saveHandlers)
            {
                saveFileCreator.SaveGame(savePath);
            }
        }
    }
}