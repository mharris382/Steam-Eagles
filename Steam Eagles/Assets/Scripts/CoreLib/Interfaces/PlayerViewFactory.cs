using CoreLib.Extensions;
using UnityEngine;

namespace CoreLib.Interfaces
{
    public class PlayerViewFactory
    {
        private readonly IPCViewFactory _viewFactory;
        private readonly IPCIdentifier _pcIdentifier;

        public PlayerViewFactory(IPCViewFactory viewFactory, IPCIdentifier pcIdentifier)
        {
            _viewFactory = viewFactory;
            _pcIdentifier = pcIdentifier;
        }
        public T GetViewForPlayer<T>(int playerNumber, T viewPrefab) where T : Component
        {
            if (viewPrefab == null)
            {
                Debug.LogError("Null viewPrefab passed to PCViewFactory.GetViewForPlayer");
                return null;
            }
            var go = GetViewForPlayer(playerNumber, viewPrefab.gameObject);
            return go.GetComponent<T>();
        }
        public GameObject GetViewForPlayer(int playerNumber, GameObject viewPrefab)
        {
            if (viewPrefab == null)
            {
                Debug.LogError("Null viewPrefab passed to PCViewFactory.GetViewForPlayer");
                return null;
            }
            return _viewFactory.Create(playerNumber, viewPrefab);
        }
        public GameObject GetViewForPlayer(GameObject playerCharacter, GameObject viewPrefab)
        {
            if (playerCharacter == null || viewPrefab == null)
            {
                Debug.LogError($"Null argument passed to PCViewFactory.GetViewForPlayer\n{playerCharacter.GetNameOrNull()}\tPrefab={viewPrefab.GetNameOrNull()}");
                return null;
            }
            int playerNumber = _pcIdentifier.IdentifyPlayerNumber(playerCharacter);
            if (playerNumber == -1)
                throw new NotPlayerGameObjectException(playerCharacter);
            return GetViewForPlayer(playerNumber, viewPrefab);
        }
    }
}