using System;
using System.Collections;
using CoreLib;
using Game;
using UniRx;
using UnityEngine;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(Canvas))]
    public class PlayerGUICanvas : MonoBehaviour
    {
        public int playerNumber;

        private Canvas _canvas;
        private PCInstance _currentPC;

        private IEnumerator Start()
        {
            _canvas = GetComponent<Canvas>();
            while (!GameManager.Instance.HasPC(playerNumber))
            {
                yield return null;
            }
            UpdatePC(GameManager.Instance.GetPC(playerNumber));
            MessageBroker.Default.Receive<PCInstanceChangedInfo>()
                .Where(t => t.playerNumber == playerNumber)
                .Select(t => t.pcInstance)
                .Subscribe(UpdatePC)
                .AddTo(this);
        }

        void UpdatePC(PCInstance pc)
        {
            _currentPC = pc;
            if(pc.IsValid())
                _canvas.worldCamera = pc.camera.GetComponent<Camera>();
        }
    }
}