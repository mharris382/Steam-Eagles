﻿using System;
using System.Collections.Generic;
using Buildings.Rooms.Tracking;
using CoreLib;
using UniRx;
using UnityEngine;
using Zenject;

namespace Players.PCController
{

    public class PCManager : IFixedTickable
    {
        private readonly PCTracker _tracker;
        private readonly PC.Factory _factory;
        private readonly CompositeDisposable _disposable;
        private readonly PC[] _pcs;
        
        public PCManager(PCTracker tracker, PC.Factory factory)
        {
            Debug.Log("Creating PCManager");
            _tracker = tracker;
            _factory = factory;
            _pcs = new PC[2];
            _disposable = new CompositeDisposable();
            _tracker.OnPCChanged.StartWith(tracker.AllPCsAndPlayerNumbers())
                .Subscribe(t => OnPlayerCharacterJoined(t.Item1, t.Item2))
                .AddTo(_disposable);
        }

        private void OnPlayerCharacterJoined(int playerNumber, PCTracker.TrackedPC trackedPCInstance)
        {
            if (_pcs[playerNumber] != null) _pcs[playerNumber].Dispose();
            _pcs[playerNumber] = _factory.Create(playerNumber, trackedPCInstance);
        }

        public void FixedTick()
        {
            foreach (var allManagedPc in GetAllManagedPcs())
            {
                allManagedPc.FixedTick();
            }
        }


        IEnumerable<PC> GetAllManagedPcs()
        {
            foreach (var pc in _pcs)
            {
                if(pc != null) yield return pc;
            }
        }
    }
    
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