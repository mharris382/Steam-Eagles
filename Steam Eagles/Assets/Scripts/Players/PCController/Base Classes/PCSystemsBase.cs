using System;
using System.Collections.Generic;
using Buildings.Rooms.Tracking;
using CoreLib.Signals;
using Players.PCController;
using UniRx;
using UnityEngine;

/// <summary>
/// base class for any system which will be created once per local player.  This class will handle the creation and disposal of the system
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PCSystemsBase<T> : IDisposable  where T : PCSystem
{
    private readonly PCTracker _pcTracker;
    private readonly PC.Factory _pcFactory;
    
    private T[] _systems;
    private PC[] _pcs;
    private IDisposable _disposable;

    public IPCTracker GetTrackerFor(int pc) => _pcTracker.GetTrackerFor(pc);

    public PCSystemsBase(PCTracker pcTracker, PC.Factory pcFactory)
    {
        _systems = new T[2];
        _pcs = new PC[2];
        _pcTracker = pcTracker;
        _pcFactory = pcFactory;
        _disposable = _pcTracker.OnPCChanged.StartWith(_pcTracker.AllPCsAndPlayerNumbers())
            .Subscribe(t => OnPCInstanceChanged(t.Item1, t.Item2));
    }

    void OnPCInstanceChanged(int playerNumber, PCTracker.TrackedPC trackedPC)
    {
        if (_systems[playerNumber] != null) _systems[playerNumber].Dispose();
        _pcs[playerNumber] = _pcFactory.Create(playerNumber, trackedPC);
        Debug.Assert(_pcs[playerNumber]!=null);
        _systems[playerNumber] = CreateSystemFor(_pcs[playerNumber]);
    }

    public abstract T CreateSystemFor(PC pc);

    public T GetSystemFor(int player) => _systems[player];
    public T GetSystemFor(PC player) => GetSystemFor(player.PlayerNumber);
    public T GetSystemFor(PCTracker.TrackedPC player) => GetSystemFor(player.PlayerNumber);
    
    public PC GetPC(int player) => _pcs[player];

    public IEnumerable<PC> GetAllPCs()
    {
        foreach (var pc in _pcs)
        {
            if(pc != null) yield return pc;
        }
    }

    public IEnumerable<T> GetAllSystems()
    {
        foreach (var system in _systems)
        {
            if(system != null) yield return system;
        }
    }
    public virtual void Dispose()
    {
        _disposable?.Dispose();
        foreach (var pcSystem in _systems) pcSystem?.Dispose();
    }

}