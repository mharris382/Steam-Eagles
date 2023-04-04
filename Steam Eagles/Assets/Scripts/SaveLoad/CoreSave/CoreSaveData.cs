namespace SaveLoad.CoreSave
{
    [System.Serializable]
    public class CoreSaveData
    {
        private readonly string[] _playerCharacterNames;
        private readonly int _scene;
        
        public int Scene => _scene;
        public string[] PlayerCharacterNames => _playerCharacterNames;
        public CoreSaveData(int scene, params string[] playerCharacterNames)
        {
            _scene = scene;
            _playerCharacterNames = playerCharacterNames;
        }
        
    }
}