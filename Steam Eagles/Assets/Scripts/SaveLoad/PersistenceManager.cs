using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.SaveLoad;
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

        void LoadGameAtPath(string loadPath)
        {
            this.SaveDirectoryPath = loadPath;
            if (!Directory.Exists(SaveDirectoryPath))
            {
                Debug.LogError($"No load path at {SaveDirectoryPath}");
                return;
            }

            var loadHandler = new LoadGameHandler(true);
            loadHandler.LoadGame(SaveDirectoryPath);
            return;
            throw new NotImplementedException();
            //Steps for loading a game
            // first find all data files in the save directory
            
            // load game core
            // load character save data
            // load inventory save data
            // load structure save data
            
            // when all data is loaded also load the scene
            
            // spawn the characters into the scenes from character save data
            // spawn the structures into the scenes from structure save data
            // spawn the inventory into the scenes from inventory save data 
            
            //now raise the event to let the game know that the game has been loaded and player devices can be assigned to their spawned characters
            
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