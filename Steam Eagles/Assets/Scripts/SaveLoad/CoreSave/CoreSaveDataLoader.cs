using UnityEngine.SceneManagement;

namespace SaveLoad.CoreSave
{
    [LoadOrder(-1000)]
    public class CoreSaveDataLoader : SaveFileLoader<CoreSaveData>
    {
        public override bool LoadData(CoreSaveData data)
        {
            SceneManager.LoadScene(data.Scene);
            throw new System.NotImplementedException();
        }
    }
}