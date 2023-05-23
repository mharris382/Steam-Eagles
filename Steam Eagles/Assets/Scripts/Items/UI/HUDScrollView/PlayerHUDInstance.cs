using System;
using UnityEngine;
using Zenject;

namespace Items.UI.HUDScrollView
{
    public class PlayerHUDInstance
    {
        private readonly PlayerHUD _playerHUD;
        private readonly Canvas _playerHUDCanvas;
        private readonly CanvasGroup _playerHUDCanvasGroup;
        private readonly CanvasRenderer _playerHUDCanvasRenderer;

        public class Factory : PlaceholderFactory<PlayerHUD, PlayerHUDInstance> { }

        public PlayerHUD PlayerHUD => _playerHUD;

        public PlayerHUDInstance(PlayerHUD playerHUD)
        {
            _playerHUD = playerHUD;
            _playerHUDCanvas = _playerHUD.GetComponent<Canvas>();
            _playerHUDCanvasRenderer = _playerHUD.GetComponent<CanvasRenderer>();
            _playerHUDCanvasGroup = _playerHUD.GetComponent<CanvasGroup>();
        }

        public void SetPlayer(int playerNumber)
        {
            //TODO: add settings for how player HUDs are displayed (based on split screen settings, etc)
            
        }
    }
}