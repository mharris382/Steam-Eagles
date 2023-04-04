using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Game;
using Players.Shared;
using SaveLoad;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.MainMenu
{
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
                var newGameSave = new NewGameSaveCreator(false);
                newGameSave.CreateNewGameSave(newGameSaveName);
                Debug.Log($"Current save path is {Application.persistentDataPath}/{PersistenceManager.Instance.SaveDirectoryPath}");
                MessageBroker.Default.Publish(new LoadGameRequestedInfo(newGameSaveName));
                
            }).AddTo(this);

        }

        private void OnEnable()
        {
            
        }
        
        private void OnDisable()
        {
            
        }

        private float[] _timeLastMoved = new float[2];

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