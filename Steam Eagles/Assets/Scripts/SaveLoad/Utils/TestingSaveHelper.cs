using System;
using UnityEngine;
using Zenject;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace SaveLoad
{
    public class TestingSaveHelper : MonoBehaviour
    {
        private GlobalSavePath _savePath;
        private GlobalSaveLoader _saveLoader;
        private Coroutine _currentOp;
        public const string TEST_SAVE_PATH_NAME_PERSISTENT_DATA_PATh = "Editor Test Save";
        public const string TEST_SAVE_PATH_NAME_REPOSITORY = "Editor Test Save";


        [EnumPaging] public SaveMode saveMode = SaveMode.SHARED_REPOSITORY;
        
        public enum SaveMode
        {
            PERSISTENT_DATA_PATH,
            SHARED_REPOSITORY
        }
        
        [Inject] void Install(GlobalSavePath savePath, GlobalSaveLoader saveLoader)
       {
           _savePath = savePath;
           _saveLoader = saveLoader;
       }


        [ButtonGroup()]
        void LoadTestGame()
        {
            StopOperation();
            var path = SetSavePathFromMode();
            _currentOp = StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var success = await _saveLoader.LoadGameAsync();
                if (!success) Debug.LogWarning($"Failed to load test save: {path}", this);
            }));
        }

        [ButtonGroup()]
        void SaveTestGame()
        {
            StopOperation();
            var path = SetSavePathFromMode();
            _currentOp = StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                var success = await _saveLoader.SaveGameAsync();
                if (!success) Debug.LogWarning($"Failed to save test save: {path}", this);
            }));
        }

        private string SetSavePathFromMode()
        {
            string path;
            switch (saveMode)
            {
                case SaveMode.PERSISTENT_DATA_PATH:
                    path = SetPath_PersistentDataPath();
                    break;
                case SaveMode.SHARED_REPOSITORY:
                    path = SetPath_SharedSaveFiles();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return path;
        }

        static string GetValidPath_PersistentDataPath()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, TEST_SAVE_PATH_NAME_PERSISTENT_DATA_PATh);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            return fullPath;
        }

        static string GetValidPath_SharedDataPath()
        {
            if (!Application.isEditor)
            {
                throw new InvalidOperationException("only use this in editor");
            }
            string dataPath = Application.dataPath;
            dataPath = dataPath.Replace('\\','/');
            var pathList = dataPath.Split('/').ToList();
            pathList.Remove(pathList.Last());
            pathList.Remove(pathList.Last());
            dataPath = string.Join("/", pathList);
            string path = Path.Combine(dataPath, "_SHARED_SAVE_FILES", TEST_SAVE_PATH_NAME_REPOSITORY);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            Debug.Log($"Shared Save Path is: {path}");
            return path;
        }
        
        private string SetPath_PersistentDataPath()
        {
            string path = GetValidPath_PersistentDataPath();
            SetPath(path, false);
            return path;
        }
        
        private string SetPath_SharedSaveFiles()
        {
            string path = GetValidPath_SharedDataPath();
            SetPath(path, true);
            return path;
        }
        
        private bool SetPath(string path, bool allowSavingOutsidePersistentDataPath)
        {
            _savePath.AllowSavingOutsidePersistentDataPath = allowSavingOutsidePersistentDataPath;
            var result = _savePath.TrySetSavePath(ref path);
            Debug.Assert(result, $"Test Saver Failed to set save path: {path}");
            return result;
        }

        private void StopOperation()
        {
            if (_currentOp != null)
            {
                StopCoroutine(_currentOp);
                _currentOp = null;
            }
        }
        
    
    }
}