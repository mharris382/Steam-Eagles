using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreLib;
using CoreLib.Entities;
using Cysharp.Threading.Tasks;
using Game;
using Players.Shared;
using SaveLoad;
using SaveLoad.CoreSave;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace UI.MainMenu
{
    public class NewGameCharacterSpawner
    {
        GameObject _builderPrefab;
        GameObject _transporterPrefab;
        
        private const string BuilderSpawnPointTag = "Builder Spawn";
        private const string TransporterSpawnPointTag = "Transporter Spawn";

        private Transform _builderDefaultSpawn;
        private Transform _transporterDefaultSpawn;
        

        public NewGameCharacterSpawner()
        {
            var tSpawnGo = GameObject.FindGameObjectWithTag(TransporterSpawnPointTag);
            var bSpawnGo = GameObject.FindGameObjectWithTag(BuilderSpawnPointTag);
            
            if (tSpawnGo != null)
                _transporterDefaultSpawn = tSpawnGo.transform;
            else 
                Debug.LogError("No Default Transporter Spawn Point Found");
            
            if (bSpawnGo != null)
                _builderDefaultSpawn = bSpawnGo.transform;
            else
                Debug.LogError("No Default Builder Spawn Point Found");
        }
        public async UniTask LoadCharacterPrefabs(bool loadBuilder, bool loadTransporter)
        {
            if (loadBuilder && loadTransporter)
            {
                SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadPrefabs();
                await UniTask.WhenAll(
                    SaveLoad.CoreSave.LoadedCharacterPrefabs.BuilderPrefabLoadOp.ToUniTask(),
                    SaveLoad.CoreSave.LoadedCharacterPrefabs.TransporterPrefabLoadOp.ToUniTask());
                _builderPrefab = SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadedBuilderPrefab;
                _transporterPrefab = SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadedTransporterPrefab;
            }
            else if (loadBuilder)
            {
                SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadBuilder();
                await SaveLoad.CoreSave.LoadedCharacterPrefabs.BuilderPrefabLoadOp.ToUniTask();
                _builderPrefab = SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadedBuilderPrefab;
            }
            else if (loadTransporter)
            {
                SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadTransporter();
                await SaveLoad.CoreSave.LoadedCharacterPrefabs.TransporterPrefabLoadOp.ToUniTask();
                _transporterPrefab = SaveLoad.CoreSave.LoadedCharacterPrefabs.LoadedTransporterPrefab;
            }
        }


        public async UniTask SpawnFromAssignment(string assignment, int index)
        {
            bool completedRequest = false;
            var request = new RequestPlayerCharacterSpawn(assignment, assignment == "Builder" ?  _builderPrefab : _transporterPrefab, index,
                _builderDefaultSpawn.localPosition)
            {
                callback = () => completedRequest = true
            };
            MessageBroker.Default.Publish(request);
            await UniTask.WaitUntil(() => completedRequest);
        }

        public GameObject SpawnBuilder()
        {
            var builderGo = GameObject.Instantiate(_builderPrefab, _builderDefaultSpawn.position, Quaternion.identity);
            builderGo.transform.parent = _builderDefaultSpawn;
            return builderGo;
        }
        public GameObject SpawnTransporter()
        {
            var transporterGo = GameObject.Instantiate(_transporterPrefab, _transporterDefaultSpawn.position, Quaternion.identity);
            transporterGo.transform.parent = _transporterDefaultSpawn;
            return transporterGo;
        }
     
    }
    public class UICharacterSelection : MonoBehaviour
    {
        public string newGameSaveName = "NewGame";
        [SerializeField, Range(0,1)]
        private float timeBetweenSelections = 0.25f;
        public bool isMainMenu = true;

        public string startGameActionMap = "Gameplay";
        public string uiActionMap = "UI";

        public Transform undecidedRoot;
        public Transform builderRoot;
        public Transform transporterRoot;
        
        public Button startButton;

        private PlayerInput[] _activeDevices;
        
        public GameObject[] deviceUIIcons;

        public UnityEvent onComplete;

        [Inject]
        public void InjectMe(GlobalSavePath savePath, GlobalSaveLoader saveLoader)
        {
            this.savePath = savePath;
            this.saveLoader = saveLoader;
        }
        
        private void Awake()
        {
            _activeDevices = new PlayerInput[2];
            

            startButton.onClick.AsObservable().Subscribe(_ =>
            {
                onComplete?.Invoke();
                foreach (var activeDevice in _activeDevices)
                {
                    if (activeDevice == null) continue;
                    activeDevice.SwitchCurrentActionMap(startGameActionMap);
                }

                StartCoroutine(UniTask.ToCoroutine(async () =>
                {
                    var loadOp = SceneManager.LoadSceneAsync("AirshipScene");
                    await loadOp;
                    
                    var newGameCharacterSpawner = new NewGameCharacterSpawner();
                    await newGameCharacterSpawner.LoadCharacterPrefabs(true, true);
                    var assignments = GameManager.Instance.GetCharacterAssignments();

                    for (int i = 0; i < 2; i++)
                    {
                        if(GameManager.Instance.PlayerHasCharacterAssigned(i))
                        {
                             var assignment = assignments[i];
                             await newGameCharacterSpawner.SpawnFromAssignment(assignment, i);
                        }
                    }
                    var entities = FindObjectsOfType<EntityInitializer>();
                    foreach (var e in entities) e.Initialize();
                    foreach (var inputGo in GameManager.Instance.GetInputs())
                    {
                        var input = inputGo.GetComponent<PlayerInput>();
                        input.SwitchCurrentActionMap("Gameplay");
                    }
                    var b = await saveLoader.SaveGameAsync("New Game");
                    if (b)
                    {
                        Debug.Log($"New Game Saved: {savePath.FullSaveDirectoryPath}");
                        CoreSaveData coreSaveData = new CoreSaveData(SceneManager.GetSceneByName("AirshipScene").buildIndex,
                            GameManager.Instance.GetCharacterAssignments());
                        var path = savePath.FullSaveDirectoryPath;
                        var coreDataSavePath = Path.Combine(path, "CoreSaveData.json");
                        var json = JsonUtility.ToJson(coreSaveData);
                        File.WriteAllText(coreDataSavePath, json);
                    }
                    else
                    {
                        Debug.LogError($"New Game failed to save: {savePath.FullSaveDirectoryPath}");
                    }
                    
                    
                }));
                // var newGameSave = new NewGameSaveCreator(false);
                // newGameSave.CreateNewGameSave(newGameSaveName);
                // PlayerPrefs.SetString("Last Save Path", newGameSaveName);
                // Debug.Log($"Current save path is {Application.persistentDataPath}/{PersistenceManager.Instance.SaveDirectoryPath}");
                // MessageBroker.Default.Publish(new LoadGameRequestedInfo(newGameSaveName));
                
            }).AddTo(this);

        }
        
        

        private void OnEnable()
        {
            
        }
        
        private void OnDisable()
        {
            
        }

        private float[] _timeLastMoved = new float[2];
        private GlobalSaveLoader saveLoader;
        private GlobalSavePath savePath;

        private void Update()
        {
            void OnSelectionChanged(string characterName, int i)
            {
                if (characterName == null)
                    MessageBroker.Default.Publish(new PlayerCharacterUnboundInfo() { playerNumber = i });
                else
                    MessageBroker.Default.Publish(new PlayerCharacterBoundInfo()
                        { playerNumber = i, character = characterName });
            }

            int[] undecidedPlayers = GetPlayers(undecidedRoot).ToArray();
            bool singlePlayerMode = GameManager.Instance.GetNumberOfPlayerDevices() <= 1;
            if (singlePlayerMode)
            {
                startButton.interactable = GameManager.Instance.CanStartGameInSingleplayer();
            }
            else
            {
                startButton.interactable = GameManager.Instance.CanStartGameInMultiplayer();
            }
            

            for (int i = 0; i < 2; i++)
            {
                var icon = deviceUIIcons[i];
                var activeGo = GameManager.Instance.GetPlayerDevice(i);
                if (activeGo == null)
                {
                    icon.SetActive(false);
                    continue;
                }
                var active = activeGo.GetComponent<PlayerInput>();
                if (active == null)
                {
                    icon.SetActive(false);
                    continue;
                }
                if(icon.activeSelf==false)icon.SetActive(true);
                if(active.currentActionMap == null || active.currentActionMap.name != uiActionMap)
                    active.SwitchCurrentActionMap(uiActionMap);
                var navigateX =active.actions["Horizontal Select"].ReadValue<float>();
                if(navigateX==0)
                    continue;
                if(navigateX>0 && CanMoveRight(icon.transform, out var rightParent, out var characterName))
                {
                    if((Time.realtimeSinceStartup-_timeLastMoved[i])<timeBetweenSelections)
                        continue;
                    _timeLastMoved[i] = Time.realtimeSinceStartup;
                    icon.transform.SetParent(rightParent);
                    Animator leftAnimator = null;
                    for (int j = 0; j < rightParent.parent.childCount; j++)
                    {
                        var child = rightParent.parent.GetChild(j);
                        leftAnimator = child.GetComponent<Animator>();
                        if (leftAnimator != null) break;
                    }
                    if (leftAnimator != null) leftAnimator.Play("OnSelect");
                    OnSelectionChanged(characterName, i);
                }
                else if(navigateX<0 && CanMoveLeft(icon.transform, out var leftParent, out var characterName2))
                {
                    if(Time.realtimeSinceStartup-_timeLastMoved[i]<timeBetweenSelections)
                        continue;
                    _timeLastMoved[i] = Time.realtimeSinceStartup;
                    icon.transform.SetParent(leftParent);
                    Animator leftAnimator = null;
                    for (int j = 0; j < leftParent.parent.childCount; j++)
                    {
                        var child = leftParent.parent.GetChild(j);
                        leftAnimator = child.GetComponent<Animator>();
                        if (leftAnimator != null) break;
                    }
                    if (leftAnimator != null) leftAnimator.Play("OnSelect");
                    OnSelectionChanged(characterName2, i);
                }
            }
            
        }


        private bool CanMoveRight(Transform icon, out Transform rightParent, out string characterName)
        {
            if (icon.parent == undecidedRoot)
            {
                rightParent = transporterRoot;
                characterName = "Transporter";
               
                return true;
            }

            if (icon.parent == builderRoot)
            {
                rightParent = undecidedRoot;
                characterName =null;
                return true;
            }

            rightParent = null;
            characterName = null;
            return false;
        }
        private bool CanMoveLeft(Transform icon, out Transform rightParent, out string characterName)
        {
            if (icon.parent == undecidedRoot)
            {
                rightParent = builderRoot;
                characterName = "Builder";
                return true;
            }

            if (icon.parent == transporterRoot)
            {
                rightParent = undecidedRoot;
                characterName = null;
                return true;
            }

            rightParent = null;
            characterName = null;
            return false;
        }


        IEnumerable<int> GetPlayers(Transform root)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                if(child.gameObject.activeSelf==false)
                    continue;
                if(int.TryParse(child.name, out var playerIndex))
                    yield return playerIndex;
            }
        }
    }
}