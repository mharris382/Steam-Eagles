using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.MainMenu
{
    [System.Obsolete]
    [Serializable]
    public class CharacterSelectWindow : MainMenuWindow
    {
           
        [Required][ChildGameObjectsOnly] public RectTransform undecidedPlayerRoot;
        [Required][ChildGameObjectsOnly] public RectTransform transporterSelectRoot;
        [Required][ChildGameObjectsOnly] public RectTransform builderSelectRoot;
        [Required][ChildGameObjectsOnly]public GameObject[] playerDeviceIcons;
        [Required][ChildGameObjectsOnly]public Button confirmSelectionButton;

        [InfoBox("@example")]
        [SerializeField, ShowInInspector]
        private string playerFormatString = "Player {0}";

        [UsedImplicitly]
        private string example => string.Format(playerFormatString, 1);
        
        public override void Init(UIMainMenu mainMenu)
        {
            confirmSelectionButton.onClick.AsObservable().Subscribe(_ =>
            {
                //mainMenu.StartGame();
            });
        }

        
        public void ShowForPlayers(int numberOfPlayers)
        {
            window.gameObject.SetActive(true);
            foreach (var playerDeviceIcon in playerDeviceIcons)
            {
                playerDeviceIcon.transform.SetParent(undecidedPlayerRoot);
            }
            for(int i = 0; i < playerDeviceIcons.Length; i++)
            {
                playerDeviceIcons[i].SetActive(i < numberOfPlayers);
            }
        }

        public void PlayerSelectedTransporter(int playerIndex)
        {
            playerDeviceIcons[playerIndex].transform.SetParent(transporterSelectRoot);
        }

        public void PlayerSelectedBuilder(int playerIndex)
        {
            playerDeviceIcons[playerIndex].transform.SetParent(builderSelectRoot);
        }

        public void PlayerSelectedUndecided(int playerIndex)
        {
            playerDeviceIcons[playerIndex].transform.SetParent(undecidedPlayerRoot);
        }

        public bool AreAllPlayersDecided(int playerCount)
        {
            for (int i = 0; i < playerCount; i++)
            {
                if (playerDeviceIcons[i].transform.parent == undecidedPlayerRoot)
                    return false;
            }
            return true;
        }


        public void UpdateSelectionWindow(int playerCount)
        {
                return;
            confirmSelectionButton.interactable = AreAllPlayersDecided(playerCount);
            int cnt = 0;
            foreach (var playerDeviceIcon in playerDeviceIcons)
            {
                playerDeviceIcon.SetActive(cnt < playerCount);
                playerDeviceIcon.name = cnt.ToString();
                cnt++;
                if (playerDeviceIcon.gameObject.activeSelf)
                {
                    var text = playerDeviceIcon.GetComponentInChildren<TextMeshProUGUI>();
                    if (text == null)
                    {
                        Debug.LogError("No text component found on player device icon", playerDeviceIcon.gameObject);
                        continue;
                    }

                    
                    text.text = string.Format(playerFormatString, cnt);
                }
            }
            
        }
    }
}