using System;
using UniRx;
using UnityEngine;

namespace Buildings
{
    public class BuildingState : MonoBehaviour
    {

        public bool?[] playersInBuilding;

        Subject<int> _playerCountChanged = new Subject<int>();

        public IObservable<int> PlayerCountChanged => _playerCountChanged;
        private void Awake()
        {
            playersInBuilding = new bool?[2]
            {
                null,
                null
            };
        }

        public void SetPlayerInBuilding(int playerNumber, bool playerInBuilding)
        {
            playerNumber = Mathf.Clamp(playerNumber, 0, 1);
            var cur = playersInBuilding[playerNumber];
            playersInBuilding[playerNumber] = playerInBuilding;
            _playerCountChanged.OnNext(CountPlayersInBuilding());
        }

        int CountPlayersInBuilding()
        {
            int cnt = 0;
            foreach (var b in playersInBuilding)
            {
                if (b.HasValue && b.Value) cnt++;
            }

            return cnt;
        }
    }
}