using System;
using Buildings.Rooms.Tracking;
using CoreLib;
using UniRx;
using UnityEngine;
using Zenject;

namespace Players.PCController
{
    public class PC : IDisposable
    {
        private readonly int _playerNumber;
        private readonly PCInstance _pc;
        public readonly CompositeDisposable DisposeWithPC;
        private readonly PCTracker.TrackedPC _trackedPCTracker;

        public int PlayerNumber => _playerNumber;
        public PCInstance PCInstance => _pc;
        
        internal PC(int playerNumber, PCTracker.TrackedPC trackedPC)
        {
            _playerNumber = playerNumber;
            _pc = trackedPC.Instance;
            _trackedPCTracker = trackedPC;
            DisposeWithPC = new CompositeDisposable();
            
        }

        public class Factory : PlaceholderFactory<int, PCTracker.TrackedPC, PC> { }

        public void Dispose()
        {
            DisposeWithPC.Dispose();
        }

        public void FixedTick()
        {
            Debug.Log($"FixedTick for PC {_playerNumber}");
        }
    }
}