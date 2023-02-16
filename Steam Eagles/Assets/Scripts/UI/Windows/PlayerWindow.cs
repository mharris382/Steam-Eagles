using System;
using CoreLib;
using Players;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// a window that is owned by a specific player
    /// </summary>
    public class PlayerWindow : Window
    {
        [Required]public SharedInt activePlayerCount;
        [Required] public Player player;


        private int PlayerIndex => player.playerNumber;
        
        private void Awake()
        {
            activePlayerCount.onValueChanged.AsObservable().Subscribe(OnPlayerCountChanged).AddTo(this);
        }

        private void OnPlayerCountChanged(int playerCount)
        {
            var parent = UIWindowCanvasManager.Instance.GetScreenParent(GetScreenSide(playerCount, PlayerIndex));
            if(parent != null)
                transform.SetParent(parent,false);
            else
            {
                Debug.LogWarning("Parent is null!");
            }
        }

        private static ScreenSide GetScreenSide(int playerCount, int playerIndex)
        {
            if (playerCount <= 1)
                return ScreenSide.FULL;
            return playerIndex == 0 ? ScreenSide.LEFT : ScreenSide.RIGHT;
        }
        
        
    }
}