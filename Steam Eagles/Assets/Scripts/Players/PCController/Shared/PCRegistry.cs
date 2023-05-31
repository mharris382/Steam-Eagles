using System;
using System.Collections.Generic;
using CoreLib;
using CoreLib.Interfaces;
using CoreLib.Signals;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// keeps a record of all PCs in the game
/// </summary>
public class PCRegistry : IPCIdentifier, IRegistry<PCInfo>
{
    private CompositeDisposable _cd;
    
    private ReactiveProperty<int> _pcCount = new ReactiveProperty<int>();

    public int PCCount
    {
        get => _pcCount.Value;
        private set => _pcCount.Value = value;
    }
    
    private readonly bool[] _hasPcs = new bool[2];
    private readonly PCInfo[] _pcs = new PCInfo[2];
    
    public PlayerInput GetPlayerInput(int player) => _hasPcs[player] ? _pcs[player].PC.input.GetComponent<PlayerInput>() : null;
    public Camera GetPlayerCamera(int player) => _hasPcs[player] ? _pcs[player].PC.camera.GetComponent<Camera>() : null;

    private Subject<int> onPCAdded = new Subject<int>();
    private Subject<int> onPCRemoved = new Subject<int>();
    private Subject<int> onPCWillChange = new Subject<int>();
    private Subject<int> onPCChanged = new Subject<int>();
    
    
    public IObservable<int> OnPCAdded => onPCAdded;
    public IObservable<int> OnPCRemoved => onPCRemoved;
    public IObservable<int> OnPCWillChange => onPCWillChange;
    public IObservable<int> OnPCChanged => onPCChanged;
    public PCRegistry()
    {
        _cd = new CompositeDisposable();
        MessageBroker.Default.Receive<PCInfo>().Subscribe(RegisterNewPC).AddTo(_cd);
    }
    public PCInstance GetInstance(int player) => _hasPcs[player] ? _pcs[player].PC : null;
    public IPCTracker GetTracker(int player) => _hasPcs[player] ? _pcs[player].PCTracker : null;
    public IEnumerable<PCInfo> GetPcs() => _pcs;
    public bool HasPc(int player) => _hasPcs[player];
    private void RegisterNewPC(PCInfo pcInfo)
    {
        int i = pcInfo.PlayerNumber;
        if (_hasPcs[pcInfo.PlayerNumber])
        {
            onPCWillChange.OnNext(i);
            var currentReference = _pcs[pcInfo.PlayerNumber];
            if (currentReference != null)
            {
                if(currentReference.PC != pcInfo.PC)
                    currentReference.PC.Dispose();
                currentReference.ResetFrom(pcInfo);
                onPCChanged.OnNext(i);
                return;
            }
        }
        
        _pcs[pcInfo.PlayerNumber] = pcInfo;
        _hasPcs[pcInfo.PlayerNumber] = true;
        onPCAdded.OnNext(i);
        
        pcInfo.PC.Disposable.Add(Disposable.Create(() => CleanupPc(pcInfo.PlayerNumber)));
        
        PCCount = GetCount();
    }

    private void CleanupPc(int pcIndex)
    {
        if (!_hasPcs[pcIndex])
        {
            throw new NotImplementedException();
        }
        _hasPcs[pcIndex] = false;
        _pcs[pcIndex].PC.Dispose();
        onPCRemoved.OnNext(pcIndex);
        PCCount = GetCount();
    }

    private int GetCount()
    {
        int cnt = 0;
        foreach (var hasPc in _hasPcs)
        {
            if (hasPc) cnt++;
        }

        return cnt;
    }

    public int IdentifyPlayerNumber(GameObject player)
    {
        for (int i = 0; i < _pcs.Length; i++)
        {
            if (IsPlayer(i, player))
            {
                return i;
            }
        }
        return -1;
    }

    private bool IsPlayer(int player, GameObject go)
    {
        if(!_hasPcs[player]) return false;
        return _pcs[player].PC.character == go || _pcs[player].PC.input == go;
    }
    public bool Register(PCInfo value)
    {
        RegisterNewPC(value);
        return true;
    }
    public bool Unregister(PCInfo value)
    {
        if (!_hasPcs[value.PlayerNumber])
        {
            return false;
        }
        CleanupPc(value.PlayerNumber);
        return true;
    }
    public IEnumerable<PCInfo> Values
    {
        get
        {
            if (_hasPcs[0]) yield return _pcs[0];
            if (_hasPcs[1]) yield return _pcs[1];
        }
    }
    public IObservable<PCInfo> OnValueAdded => OnPCAdded.Select(t => _pcs[t]);
    public IObservable<PCInfo> OnValueRemoved => OnPCRemoved.Select(t => _pcs[t]);
    public IReadOnlyReactiveProperty<int> ValueCount => _pcCount;
    
    
}