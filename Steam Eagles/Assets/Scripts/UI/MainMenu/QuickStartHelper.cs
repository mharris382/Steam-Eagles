using System;
using System.IO;
using CoreLib;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using Game;
using SaveLoad.CoreSave;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace UI.MainMenu
{
    /// <summary>
    /// helper to quickly skip the character selection process when starting a new game for the purpose of testing
    /// </summary>
    public class QuickStartHelper : MonoBehaviour
    {
        
        public string startGameActionMap = "Gameplay";
        public string uiActionMap = "UI";
        private PlayerInput[] _activeDevices;
        private GlobalSaveLoader _saveLoader;
        private GlobalSavePath _savePath;
        
        
        public KeyCode quickstartKey = KeyCode.F1;
        public string quickStartCharacter = "Builder";
        public void InjectMe(GlobalSavePath savePath, GlobalSaveLoader saveLoader)
        {
            this._savePath = savePath;
            this._saveLoader = saveLoader;
        }

        private bool started = false;

        private void Update()
        {
            if (Input.GetKeyDown(quickstartKey) && !started)
            {
                started = true;
                MessageBroker.Default.Publish(new PlayerCharacterBoundInfo()
                    { playerNumber = 0, character = quickStartCharacter });
                StartCoroutine(Quickstart().ToCoroutine());
            }
        }

        async UniTask Quickstart()
        {
            var loadOp = SceneManager.LoadSceneAsync("AirshipScene");
            await loadOp;
            var newGameCharacterSpawner = new NewGameCharacterSpawner();
            var prefabLoadOp = newGameCharacterSpawner.LoadCharacterPrefabs(true, false);

            await UniTask.WhenAll(loadOp.ToUniTask(), prefabLoadOp);

            //spawn character assignments
            var assignments = GameManager.Instance.GetCharacterAssignments();
            Debug.Assert(GameManager.Instance.PlayerHasCharacterAssigned(0));
            for (int i = 0; i < 2; i++)
            {
                if(GameManager.Instance.PlayerHasCharacterAssigned(i))
                {
                    var assignment = assignments[i];
                    await newGameCharacterSpawner.SpawnFromAssignment(assignment, i);
                }
            }
            
            //initialize entities (usually done by load system, but this is a new game)
            var entities = FindObjectsOfType<EntityInitializer>();
            foreach (var e in entities) e.Initialize();
            
            //switch input maps to gameplay
            foreach (var inputGo in GameManager.Instance.GetInputs())
            {
                var input = inputGo.GetComponent<PlayerInput>();
                input.SwitchCurrentActionMap("Gameplay");
            }
            
            
            var b = await _saveLoader.SaveGameAsync("New Game");
            if (b)
            {
                Debug.Log($"New Game Saved: {_savePath.FullSaveDirectoryPath}");
                CoreSaveData coreSaveData = new CoreSaveData(SceneManager.GetSceneByName("AirshipScene").buildIndex,
                    GameManager.Instance.GetCharacterAssignments());
                var path = _savePath.FullSaveDirectoryPath;
                var coreDataSavePath = Path.Combine(path, "CoreSaveData.json");
                var json = JsonUtility.ToJson(coreSaveData);
                File.WriteAllText(coreDataSavePath, json);
            }
            else
            {
                Debug.LogError($"New Game failed to save: {_savePath.FullSaveDirectoryPath}");
            }

        }
        
    }
}