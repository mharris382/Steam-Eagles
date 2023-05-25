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

    private Subject<int> onPCAdded = new Subject<int>();
    private Subject<int> onPCRemoved = new Subject<int>();
    private Subject<int> onPCWillChange = new Subject<int>();
    private Subject<int> onPCChanged = new Subject<int>();
    
    
    public IObservable<int> OnPCAdded => onPCAdded;
    public IObservable<int> OnPCRemoved => onPCRemoved;

    public IObservable<int> OnPCWillChange => onPCWillChange;

    public IObservable<int> OnPCChanged => onPCChanged;


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
        int i = pcCreatedInfo.PlayerNumber;
        if (_hasPcs[pcCreatedInfo.PlayerNumber])
        {
            onPCWillChange.OnNext(i);
            var currentReference = _pcs[pcCreatedInfo.PlayerNumber];
            if (currentReference != null)
            {
                if(currentReference.PC != pcCreatedInfo.PC)
                    currentReference.PC.Dispose();
                currentReference.ResetFrom(pcCreatedInfo);
                onPCChanged.OnNext(i);
                return;
            }
        }
        
        _pcs[pcCreatedInfo.PlayerNumber] = pcCreatedInfo;
        _hasPcs[pcCreatedInfo.PlayerNumber] = true;
        onPCAdded.OnNext(i);
        
        pcCreatedInfo.PC.Disposable.Add(Disposable.Create(() =>
        {
            CleanupPc(pcCreatedInfo.PlayerNumber);
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