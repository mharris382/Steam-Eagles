using System;
using UnityEngine;

namespace CoreLib.Interfaces
{
    public interface IPCViewFactory
    {
        public GameObject Create(int playerNumber, GameObject viewPrefab);
    }

    public interface IPCIdentifier
    {
        int IdentifyPlayerNumber(GameObject player);
    }


    public class NotPlayerGameObjectException : Exception
    {
        public NotPlayerGameObjectException(GameObject expectedPlayerGameObject) : base(
            $"GameObject {expectedPlayerGameObject.name} is not a player character")
        {
            Debug.LogError($"GameObject {expectedPlayerGameObject.name} is not a player character", expectedPlayerGameObject);
        }
    }
}