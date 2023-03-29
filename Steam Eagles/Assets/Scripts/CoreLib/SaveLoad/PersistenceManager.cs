using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

namespace CoreLib.SaveLoad
{
    public class PersistenceManager : Singleton<PersistenceManager>
    {
        public string presetPath = "Editor Test";
        public bool usePreset = false;
        public string SaveDirectoryPath { get; private set; }
        
        public event Action<string> GameSaved;
        public event Action<string> GameLoaded;
        
        public void Initialize(string saveDirectoryPath)
        {
            SaveDirectoryPath = saveDirectoryPath;
            
        }

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

        void LoadGameAtPath(string loadPath)
        {
            this.SaveDirectoryPath = loadPath;
            if (!Directory.Exists(SaveDirectoryPath))
            {
                Debug.LogError($"No load path at {SaveDirectoryPath}");
                return;
            }
            GameLoaded?.Invoke(SaveDirectoryPath);
            //if we are in the main menu then we should also load the game scene after storing the path locally
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                StartCoroutine(LoadScene());
            }
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
    }
}