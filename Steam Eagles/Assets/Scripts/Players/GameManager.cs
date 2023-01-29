using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class GameManager : MonoBehaviour
    {
        [ValidateInput(nameof(ValidateWrappers))]
        [BoxGroup("Players")]  public List<PlayerWrapper> players;
        [BoxGroup("Players")]  public CameraAssignments cameraAssignments;
        [BoxGroup("Players")]  public CharacterAssignments characterAssignments;

        public GameObject waitingForFirstPlayerToJoinGUI;
        public GameObject playerDisconnectedGUI;
        
        private ReactiveCollection<PlayerWrapper> _joinedPlayers = new ReactiveCollection<PlayerWrapper>();


        IDisposable _singlePlayerCameraSubscription;
        IPlayerDependencyResolver<Camera> CameraResolver => cameraAssignments;
        
        bool ValidateWrappers(List<PlayerWrapper> wrappers)
        {
            if (wrappers.Count < 2) return false;
            for (int i = 0; i < wrappers.Count; i++)
            {
                if (wrappers[i] == null) return false;
                if(wrappers[i].player == null) return false;
                if(wrappers[i].player.characterTransform==null) return false;
                if(wrappers[i].player.playerNumber != i) return false;
            }
            return true;
        }


        public void OnPlayerLeft(PlayerInput obj)
        {
            Debug.Log($"Player {obj.playerIndex} Left");
            _joinedPlayers.Remove(players[obj.playerIndex]);
        }
        public void OnPlayerJoined(PlayerInput obj)
        {
            var id = obj.playerIndex;
            if (_joinedPlayers.Count > 0)
            {
                SwitchToMultiPlayerCamera();
            }
            try
            {
                var wrapper = players[id];
                var input = obj.GetComponent<PlayerCharacterInput>();
                
                var camera = cameraAssignments.GetDependency(id);
                Debug.Assert(camera.Value != null, "Camera Assignment was returned with null camera!", this);
                Debug.Assert(wrapper.player.playerCamera == camera, "wrapper.player.playerCamera != camera", this);
                
                var character = characterAssignments.GetDependency(id);
                Debug.Assert(character.CompareTag(wrapper.player.characterTag), $"Player {wrapper.player} assigned the wrong character {character.name} or Character tag is incorrect", this);
                
                wrapper.BindToPlayer(input);
                wrapper.AssignCamera(camera.Value);
                wrapper.AssignCharacter(character);
                Debug.Assert(wrapper.IsFullyBound);
                
                _joinedPlayers.Add(wrapper);
            }
            catch (Exception e)
            {
                Debug.LogError($"Player {obj.playerIndex} failed to join\n{e.GetType()} \n{e.Message}");
                throw;
            }
        }


        private void Start()
        {
            waitingForFirstPlayerToJoinGUI.SetActive(true);
            
            _joinedPlayers.ObserveAdd()
                .Subscribe(t =>
                {
                    HideMessageGUIs();
                })
                .AddTo(this);

            _joinedPlayers.ObserveCountChanged()
                .Where(t => t == 0)
                .Subscribe(_ =>
                {
                    waitingForFirstPlayerToJoinGUI.gameObject.SetActive(true);
                })
                .AddTo(this);
            _joinedPlayers.ObserveCountChanged()
                .Where(t => t == 1)
                .Subscribe(_ =>
                {
                    HideMessageGUIs();
                    SwitchToSinglePlayerCamera();
                })
                .AddTo(this);

            _joinedPlayers.ObserveCountChanged()
                .Where(t => t > 1)
                .Subscribe(_ =>
                {
                    HideMessageGUIs();
                    
                    
                })
                .AddTo(this);
        }


        void SwitchToSinglePlayerCamera()
        {
            if (_singlePlayerCameraSubscription != null)
            {
                _singlePlayerCameraSubscription.Dispose();
                _singlePlayerCameraSubscription = null;
            }

            var cd = new CompositeDisposable();
            
            cameraAssignments.DisableMultiPlayerCameras().AddTo(cd);
            _joinedPlayers[0].OverrideCamera(CameraResolver.GetDependency(0)).AddTo(cd);
            
            _singlePlayerCameraSubscription = cd;
        }

        void SwitchToMultiPlayerCamera()
        {
            if (_singlePlayerCameraSubscription != null)
            {
                _singlePlayerCameraSubscription.Dispose();
                _singlePlayerCameraSubscription = null;
            }
        }
        

        private void HideMessageGUIs()
        {
            waitingForFirstPlayerToJoinGUI.gameObject.SetActive(false);
            playerDisconnectedGUI.gameObject.SetActive(false);
        }
    }


    [Serializable]
    public class CharacterAssignments : IPlayerDependencyResolver<CharacterState>
    {
        [SerializeField] public List<CharacterAssignment> characterAssignments;
        
        public CharacterState GetDependency(int playerNumber)
        {
            var assignment = characterAssignments[playerNumber];
            var character = assignment.InstantiateCharacter();
            return character.GetComponent<CharacterState>();
        }
    }
}