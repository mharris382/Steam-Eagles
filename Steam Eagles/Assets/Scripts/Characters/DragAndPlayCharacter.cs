using System;
using System.Collections;
using CoreLib;
using Game;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class DragAndPlayCharacter : MonoBehaviour
    {
        [Range(0,1)] public int playerNumber = 0;
        public Transform spawnPosition;
        public GameObject characterPrefab;
        
        [ Required, SceneObjectsOnly] public GameObject targetBuilding;

        private const string BUILDING_ASSEMBLY_QUALIFIED_NAME =
            "Buildings.Building, SteamEagles.Buildings, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

        


        [ShowInInspector, ReadOnly]
        public string CharacterName => characterPrefab == null ? "NULL" : characterPrefab.tag;


        private void Awake()
        {
            spawnPosition ??= transform;
        }

        private IEnumerator Start()
        {
            while (GameManager.Instance.PlayerHasJoined(playerNumber) == false)
            {
                Debug.Log($"waiting for player {playerNumber} device to join", this);
                yield return null;
            }

            var spawnPositionLocal = targetBuilding.transform.InverseTransformPoint(spawnPosition.position);
            var request = new RequestPlayerCharacterSpawn(CharacterName, characterPrefab, playerNumber, spawnPositionLocal)
                {
                    Building = targetBuilding
                };
            MessageBroker.Default.Publish(request);
        }
    }
}