using UnityEngine;

namespace CoreLib.SaveLoad
{
    public class PersistenceManager : Singleton<PersistenceManager>
    {
        
        public string SaveDirectoryPath { get; private set; }
        
        
        public void Initialize(string saveDirectoryPath)
        {
            SaveDirectoryPath = saveDirectoryPath;
        }
    }
}