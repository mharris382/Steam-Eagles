using System;
using UnityEngine;

namespace CoreLib.Signals
{
    public class PCCreatedInfo
    {
        public int PlayerNumber { get; private set; }
        public PCInstance PC { get; private set; }
        public IPCTracker PCTracker { get; private set; }

        public PCCreatedInfo(int playerNumber, PCInstance pc, IPCTracker pcTracker)
        {
            PlayerNumber = playerNumber;
            PC = pc;
            PCTracker = pcTracker;
        }

        public void Reset(int playerNumber, PCInstance pc, IPCTracker tracker)
        {
            PlayerNumber = playerNumber;
            PC = pc;
            PCTracker = tracker;
        }

        public void ResetFrom(PCCreatedInfo otherInfo)
        {
            Reset(otherInfo.PlayerNumber, otherInfo.PC, otherInfo.PCTracker);
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