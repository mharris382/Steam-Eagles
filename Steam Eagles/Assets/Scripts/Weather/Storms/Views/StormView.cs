using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Weather.Storms.Views
{
    public class StormView : MonoBehaviour
    {
        private Storm _storm;
        
        
        
        
        [ShowInInspector] public bool hasStormBeenAssigned => _storm != null;


        private PlayerSpecificStormView[] _playerSpecificStormViews = new PlayerSpecificStormView[2]{null, null};
        private bool[] _playerInStorm = new bool[2];
        private PlayerSpecificStormView.Factory _factory;

        [InjectOptional]
        private Storm Storm
        {
            get => _storm;
            set
            {
                _storm = value;
                Debug.Log("StormView: Storm Assigned",this);
            }
        }

        [Inject]
        public void InjectMe(PlayerSpecificStormView.Factory factory)
        {
            this._factory = factory;
        }

        
        public void AssignStorm(Storm storm)
        {
            if(storm != null && _storm != storm)
                _storm = storm;
        }

        public void NotifyPlayerEntered(int playerIndex, PCStormSubject pcStormSubject)
        {
            _playerInStorm[playerIndex] = true;
            if(_playerSpecificStormViews[playerIndex] == null)
            {
                _playerSpecificStormViews[playerIndex] = _factory.Create();
                _playerSpecificStormViews[playerIndex] .Assign(playerIndex, pcStormSubject.Camera);
            }
            gameObject.SetActive(true);
        }
        
        public void NotifyPlayerExited(int playerIndex, PCStormSubject pcStormSubject)
        {
            _playerInStorm[playerIndex] = false;
            if (PlayersInStorm == 0)
            {
                //TODO: clear view because no players are in the storm
                gameObject.SetActive(false);
            }
        }
        int PlayersInStorm
        {
            get
            {
                int cnt = 0;
                foreach (var b in _playerInStorm)
                {
                    if (b) cnt++;
                }
                return cnt;
            }
        }
        
        
        
        public class AltFactory : PlaceholderFactory<StormView> { }
    }
}