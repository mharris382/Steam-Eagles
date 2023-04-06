using System;
using System.Collections;
using System.Collections.Generic;
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

        protected override void Init()
        {
            if (usePreset)
            {
                SaveDirectoryPath = presetPath;
            }
            base.Init();
        }

        private void Start()
        {
            
            MessageBroker.Default.Receive<SaveGameRequestedInfo>()
                .Select(t => t.savePath)
                .Subscribe(SaveGameAtPath).AddTo(this);

            MessageBroker.Default.Receive<LoadGameRequestedInfo>()
                .Select(t => t.loadPath)
                .Subscribe(LoadGameAtPath).AddTo(this);

            MessageBroker.Default.Receive<CharacterSpawnedInfo>()
                .DelayFrame(1)
                .Subscribe(info =>
                {
                    var position = SpawnDatabase.Instance.GetSpawnPointForScene(info.characterName, SaveDirectoryPath);
                    position.z = 0;
                    Debug.Log($"Loaded {position} from path {SaveDirectoryPath}");
                    info.character.transform.localPosition = position;
                    var go = GameObject.FindWithTag($"{info.characterName} Spawn");
                    if (go != null)
                    {
                        info.character.transform.parent = go.transform.parent;
                        info.character.transform.localPosition = position;
                        info.character.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    }
                }).AddTo(this);
        }


        void SaveGameAtPath(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Debug.Log($"Save Directory does not exist, creating one now at {savePath}");
                Directory.CreateDirectory(savePath);
            }
            OnGameSaved(savePath);
            PlayerPrefs.SetString("Last Save Path", savePath);
        }

        private Coroutine _loadRoutine;
        void LoadGameAtPath(string loadPath)
        {
            if (_loadRoutine == null)
            {
                _isLoading = false;
            }
            if (IsLoading)
            {
                Debug.LogError($"Already loading a game from path: {loadPath}",this);
                return;
            }

            if (!loadPath.Contains(Application.persistentDataPath))
            {
                loadPath = $"{Application.persistentDataPath}/{loadPath}";
            }
            
            SaveDirectoryPath = loadPath;
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
}