using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLib;
using CoreLib.SaveLoad;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Directory = System.IO.Directory;

namespace SaveLoad
{
    public class PersistenceManager : Singleton<PersistenceManager>
    {
        public string presetPath = "Editor Test";
        public bool usePreset = false;


        private DirectoryPathGenerator _pathGenerator;
        private DirectoryPathGenerator PathGenerator => _pathGenerator ??= new DirectoryPathGenerator();
        private bool _isLoading;
        public bool IsLoading => _isLoading;
        
        public string SaveDirectoryPath { get; private set; }
        public static string SavePath => Instance.SaveDirectoryPath;
        public event Action<string> GameSaved;
        public event Action<string> GameLoaded;
        public void Initialize(string saveDirectoryPath)
        {
            SaveDirectoryPath = saveDirectoryPath;
            
        }

        public override bool DestroyOnLoad => false;

        private bool inited = false;

        private void OnDestroy()
        {
            inited = false;
        }

        protected override void Init()
        {
            if (usePreset)
            {
                SaveDirectoryPath = presetPath;
            }
            this.saveHistory = new SaveHistoryLoader();
            GameSaved+= saveHistory.OnSave;
            base.Init();
            inited = true;
            
            MessageBroker.Default.Receive<SaveGameRequestedInfo>()
                .Select(t => t.savePath)
                .Subscribe(SaveGameAtPath)
                .AddTo(this);

            MessageBroker.Default.Receive<LoadGameRequestedInfo>()
                .Subscribe(LoadGameRequest)
                .AddTo(this);

            MessageBroker.Default.Receive<CharacterSpawnedInfo>()
                .DelayFrame(1)
                .Subscribe(info =>
                {
                    StartCoroutine(SpawnCharacter(info.characterName, info.character));
                    
                }).AddTo(this);
        }

        IEnumerator SpawnCharacter(string characterName, GameObject character)
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                var pos = await SpawnDatabase.Instance.GetSpawnPoint(characterName);
                pos.z = 0;
                Debug.Log($"Loaded {pos} from path {SaveDirectoryPath}");
                character.transform.localPosition = pos;
                var go = GameObject.FindWithTag($"{characterName} Spawn");
                if (go != null)
                {
                    character.transform.parent = go.transform.parent;
                    character.transform.localPosition = pos;
                    character.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                }
            });
        }

        private void Start()
        {
            if (!inited)
            {
                Init();
            }
        }

        void SaveGameAtPath(string savePath)
        {
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = GetLastSavePath();
            }
            Debug.Log($"Saving Game at path: {savePath}");
            savePath = GetPathSafe(savePath);
            var prevPath = savePath;
            savePath = PathGenerator.GenerateUniquePath(savePath);
            Debug.Log($"Original Path: {prevPath}\nUnique Path:{savePath.Bolded()}");
            if (!Directory.Exists(savePath))
            {
                Debug.Log($"Save Directory does not exist, creating one now at {savePath}");
                Directory.CreateDirectory(savePath);
            }

            PlayerPrefs.SetString("Last Save Path", savePath);
            OnGameSaved(savePath);
        }

        void SaveGameAtAutoPath(string lastSavePath)
        {
            string newPath = GetPathSafe(lastSavePath);
            DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        private string GetLastSavePath()
        {
            return saveHistory.GetSavePaths().FirstOrDefault(t => !string.IsNullOrEmpty(t)) ?? PlayerPrefs.GetString("Last Save Path");
        }

        private class SaveHistoryLoader
        {
            private readonly SaveHistory _saveHistory;

            public SaveHistoryLoader()
            {
                var path = SaveHistory.GetSaveHistorySavePath();
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _saveHistory = JsonUtility.FromJson<SaveHistory>(json);
                }
                else
                {
                    _saveHistory = new SaveHistory();
                }
            }

            public void OnSave(string path)
            {
                if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                {
                    _saveHistory.savePaths.Add(path);
                    var newJson = JsonUtility.ToJson(_saveHistory);
                    File.WriteAllText(SaveHistory.GetSaveHistorySavePath(), newJson);
                }
            }
            
            /// <summary>
            /// return the last 10 save paths
            /// </summary>
            /// <returns></returns>
            public IEnumerable<string> GetSavePaths()
            {
                var count = 0;
                for (int i = _saveHistory.savePaths.Count - 1; i >= 0; i--)
                {
                    if (count > 10)
                    {
                        break;
                    }
                    count++;
                    yield return _saveHistory.savePaths[i];
                }
            }
        }
        
        [Serializable]
        private class SaveHistory
        {
            public static string GetSaveHistorySavePath() => Path.Combine(Application.persistentDataPath, "SaveHistory.json");
            public List<string> savePaths = new List<string>();
        }

        string GetPathSafe(string path)
        {
            if (!path.Contains(Application.persistentDataPath))
            {
                path = Path.Combine(Application.persistentDataPath, path);
            }
            return path;
        }

        private Coroutine _loadRoutine;
        private SaveHistoryLoader saveHistory { get; set; } 

        public void LoadGameRequest(LoadGameRequestedInfo info)
        {
            Debug.Log($"Received Load Game Request: {info.loadPath}");
            LoadGameAtPath(info.loadPath);
        }
        void LoadGameAtPath(string loadPath)
        {
            loadPath = GetPathSafe(loadPath);
            if (_loadRoutine == null)
            {
                _isLoading = false;
            }
            if (IsLoading)
            {
                Debug.LogError($"Already loading a game from path: {loadPath}",this);
                return;
            }
            loadPath= loadPath.Replace('\\','/');
            SaveDirectoryPath = loadPath;
            PlayerPrefs.SetString("Last Save Path", loadPath);
            if (!Directory.Exists(SaveDirectoryPath))
            {
                Debug.LogError($"No load path at {SaveDirectoryPath}");
                return;
            }

            //var loadHandler = new LoadGameHandler(true);
            //loadHandler.LoadGame(SaveDirectoryPath);
            
            var loadHandler = new AsyncLoadGameHandler(true);
            StartCoroutine(LoadAsync(loadHandler));
        }

        IEnumerator LoadAsync(AsyncLoadGameHandler loadHandler)
        {
            _isLoading = true;
            Debug.Log($"Starting load from path: {SaveDirectoryPath}");
            
            yield return UniTask.ToCoroutine(async () => await loadHandler.LoadAsync(SaveDirectoryPath));
            
            Debug.Log($"Finished loading from path: {SaveDirectoryPath}");
            _isLoading = false;
        }
        
        IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync(1);
            GameLoaded?.Invoke(SaveDirectoryPath);
            while (GameObject.FindWithTag("Builder")==null)
            {
                yield return null;
            }
            yield return null;
            var go = GameObject.FindWithTag("Builder");
            go.transform.localPosition = SpawnDatabase.Instance.LoadSpawnPointForPath(go.tag, SaveDirectoryPath);
            GameLoaded?.Invoke(SaveDirectoryPath);
        }

        protected virtual void OnGameSaved(string obj)
        {
            GameSaved?.Invoke(obj);
        }



        public static IEnumerable<string> GetAllGameSaves()
        {
            var root = Application.persistentDataPath;
            foreach (var subfolder in Directory.GetDirectories(root))
            {
                yield return subfolder;
            }
        }
    }
    
    public class DirectoryPathGenerator
    {
        public string GenerateUniquePath(string path)
        {
            if (Directory.Exists(path))
            {
                // Extract the original timestamp from the path (if it exists)
                path = GetCleanPath(path);
                if (string.IsNullOrEmpty(path))
                {
                    path = "New Save";
                }
                
                // Generate new path using current time
                if (path.EndsWith('-'))
                {
                    path = path.Remove(path.Length - 1);
                }
                string newDirectory = $"{path}-{DateTime.Now:MM_dd_yyyy HH mm}";
                return newDirectory;
                // Recursively call the function with the new path

            }

            return path;
        }

        private string GetCleanPath(string path)
        {
            string[] pathSegments = path.Split('-');
            if (pathSegments.Length == 1)
            {
                return path;
            }
            return path.Replace(pathSegments[^1], "");
        }
        private string GetTimestampFromPath(string path)
        {
            string[] pathSegments = path.Split('-');
            
            if (pathSegments.Length > 1)
            {
                string lastSegment = pathSegments[pathSegments.Length - 1];
                DateTime timestamp;
                if (DateTime.TryParseExact(lastSegment, "yyyyMMdd_HHmmss", null, System.Globalization.DateTimeStyles.None, out timestamp))
                {
                    return lastSegment;
                }
            }

            return string.Empty;
        }
    }
}