using System;
using CoreLib;
using CoreLib.Signals;
using UniRx;
/// <summary>
/// keeps a record of all PCs in the game
/// </summary>
public class GlobalPCInfo
{
    private CompositeDisposable _cd;
    
    private ReactiveProperty<int> _pcCount = new ReactiveProperty<int>();

    public int PCCount
    {
        get => _pcCount.Value;
        private set => _pcCount.Value = value;
    }
    
    private readonly bool[] _hasPcs = new bool[2];
    private readonly PCCreatedInfo[] _pcs = new PCCreatedInfo[2];
    public GlobalPCInfo()
    {
        _cd = new CompositeDisposable();
        MessageBroker.Default.Receive<PCCreatedInfo>().Subscribe(RegisterNewPC).AddTo(_cd);
    }

    public PCInstance GetInstance(int player) => _hasPcs[player] ? _pcs[player].PC : null;
    public IPCTracker GetTracker(int player) => _hasPcs[player] ? _pcs[player].PCTracker : null;

    
    public bool HasPc(int player) => _hasPcs[player];
    void RegisterNewPC(PCCreatedInfo pcCreatedInfo)
    {
        if (_hasPcs[pcCreatedInfo.PlayerNumber])
        {
            throw new NotImplementedException();
        }

        _pcs[pcCreatedInfo.PlayerNumber] = pcCreatedInfo;
        _hasPcs[pcCreatedInfo.PlayerNumber] = true;
        
        pcCreatedInfo.PC.Disposable.Add(Disposable.Create(() => CleanupPc(pcCreatedInfo.PlayerNumber)));
        
        PCCount = GetCount();
    }

    void CleanupPc(int pcIndex)
    {
        if (!_hasPcs[pcIndex])
        {
            throw new NotImplementedException();
        }
        _hasPcs[pcIndex] = false;
        _pcs[pcIndex] = null;
        
        PCCount = GetCount();
    }

    int GetCount()
    {
        int cnt = 0;
        foreach (var hasPc in _hasPcs)
        {
            if (hasPc) cnt++;
        }

        return cnt;
    }
}