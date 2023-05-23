using System;
using UnityEngine;

namespace CoreLib.Signals
{
    public class PCCreatedInfo
    {
        public int PlayerNumber { get; }
        public PCInstance PC { get; }
        public IPCTracker PCTracker { get; }

        public PCCreatedInfo(int playerNumber, PCInstance pc, IPCTracker pcTracker)
        {
            PlayerNumber = playerNumber;
            PC = pc;
            PCTracker = pcTracker;
        }
    }

    /// <summary>
    /// tracks the room a specific PC is in
    /// </summary>
    public interface IPCTracker
    {
        //TODO:
        GameObject GetCurrentRoom();
        GameObject GetCurrentBuilding();

        public IObservable<GameObject> OnPcRoomChanged(bool includeCurrent = false);
        public IObservable<GameObject> OnPcBuildingChanged(bool includeCurrent = false);
    }
}