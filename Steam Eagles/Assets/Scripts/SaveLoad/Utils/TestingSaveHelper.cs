using UnityEngine;
using Zenject;
using System.IO;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace SaveLoad
{
    public class TestingSaveHelper : MonoBehaviour
    {
        private GlobalSavePath _savePath;
        private GlobalSaveLoader _saveLoader;
        private Coroutine _currentOp;
        public const string TEST_SAVE_PATH_NAME_PERSISTENT_DATA_PATh = "Editor Test Save";

        [Inject] void Install(GlobalSavePath savePath, GlobalSaveLoader saveLoader)
       {
           _savePath = savePath;
           _saveLoader = saveLoader;
       }


        [ButtonGroup()]
        void LoadTestGame()
        {
            var path = SetPath();
            StopOperation();
            _currentOp = StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var success = await _saveLoader.LoadGameAsync();
                if (!success) Debug.LogWarning($"Failed to load test save: {path}", this);
            }));
        }

        [ButtonGroup()]
        void SaveTestGame()
        {
            var path = SetPath();
            StopOperation();
            _currentOp = StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var success = await _saveLoader.SaveGameAsync();
                if (!success) Debug.LogWarning($"Failed to save test save: {path}", this);
            }));
        }

        string GetValidPath()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, TEST_SAVE_PATH_NAME_PERSISTENT_DATA_PATh);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        private string SetPath()
        {
            string path = GetValidPath();
            var result = _savePath.TrySetSavePath(ref path);
            Debug.Assert(result, $"Test Saver Failed to set save path: {path}");
            return path;
        }

        void StopOperation()
        {
            if (_currentOp != null)
            {
                StopCoroutine(_currentOp);
                _currentOp = null;
            }
        }
        
    
    }
}