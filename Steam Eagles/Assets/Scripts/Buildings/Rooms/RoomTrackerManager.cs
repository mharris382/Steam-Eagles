using System.Collections.Generic;
using CoreLib;
using UniRx;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    public class RoomTrackerManager : Singleton<RoomTrackerManager>
    {
        private Dictionary<string, CharacterRoomTracker> roomTrackers = new Dictionary<string, CharacterRoomTracker>();
        protected override void Init()
        {
            MessageBroker.Default.Receive<CharacterSpawnedInfo>()
                .Subscribe(info =>
                {
                    Debug.Log($"Registering Room Tracker for {info.characterName}", info.character);
                    var tracker = GetRoomTracker(info.characterName);
                    UpdateTrackerPosition(info.character, tracker);
                }).AddTo(this);
            
            base.Init();
        }
        
        
        private CharacterRoomTracker GetRoomTracker(string characterName)
        {
            if(roomTrackers.TryGetValue(characterName, out var roomTracker))
                return roomTracker;
            var go = new GameObject($"{characterName} RoomTracker");
            go.transform.SetParent(this.transform);
            
            Debug.Log($"Created Room Tracker for {characterName}",this);
            roomTracker = go.AddComponent<CharacterRoomTracker>();
            roomTracker.characterName = characterName;
            roomTrackers.Add(characterName, roomTracker);
            return roomTracker;
        }

        private void UpdateTrackerPosition(GameObject characterGo, CharacterRoomTracker roomTracker)
        {
            Observable.EveryUpdate()
                .TakeUntilDestroy(characterGo)
                .Subscribe(_ =>
                {
                    roomTracker.transform.position = characterGo.transform.position;
                })
                .AddTo(this);
        }
    }
}