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
            Debug.Log($"Saving Game at path: {savePath}");
            savePath = GetPathSafe(savePath);
            if (!Directory.Exists(savePath))
            {
                Debug.Log($"Save Directory does not exist, creating one now at {savePath}");
                Directory.CreateDirectory(savePath);
            }

            PlayerPrefs.SetString("Last Save Path", savePath);
            OnGameSaved(savePath);
        }

        string GetPathSafe(string path)
        {
            if (!path.Contains(Application.persistentDataPath))
            {
                path = $"{Application.persistentDataPath}/{path}";
            }
            return path;
        }

        private Coroutine _loadRoutine;
        void LoadGameRequest(LoadGameRequestedInfo info)
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
}