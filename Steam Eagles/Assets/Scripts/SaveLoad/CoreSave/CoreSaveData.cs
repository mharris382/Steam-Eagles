using UnityEngine;

namespace SaveLoad.CoreSave
{
    [System.Serializable]
    public class CoreSaveData
    {
        [SerializeField] private string[] _playerCharacterNames;
        [SerializeField] private int _scene;
        
        public int Scene => _scene;
        public string[] PlayerCharacterNames => _playerCharacterNames;
        public CoreSaveData(int scene, params string[] playerCharacterNames)
        {
            _scene = scene;
            _playerCharacterNames = playerCharacterNames;
        }
        
    }
}