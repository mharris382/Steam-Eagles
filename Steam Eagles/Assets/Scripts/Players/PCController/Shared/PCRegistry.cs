using System;
using CoreLib;
using CoreLib.Signals;
using UniRx;
/// <summary>
/// keeps a record of all PCs in the game
/// </summary>
public class PCRegistry
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

    
    
    public bool HasPc(int player) => _hasPcs[player];
    void RegisterNewPC(PCInfo pcInfo)
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
        
        pcInfo.PC.Disposable.Add(Disposable.Create(() =>
        {
            CleanupPc(pcInfo.PlayerNumber);
            onPCRemoved.OnNext(i);
        }));
        
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