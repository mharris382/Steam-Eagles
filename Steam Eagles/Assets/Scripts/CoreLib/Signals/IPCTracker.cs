using System;
using UnityEngine;

namespace CoreLib.Signals
{
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