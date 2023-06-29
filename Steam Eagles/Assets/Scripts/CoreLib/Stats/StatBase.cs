using System;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

public abstract class StatBase : MonoBehaviour
{
    [SerializeField] private IntReactiveProperty maxValue = new IntReactiveProperty(10);
    
    private StatManager _statManager;
    private DynamicReactiveProperty<int> _currentValue2 = new();
    public StatManager StatManager => GetStatManager();
    
    public virtual int MaxValue
    {
        get => maxValue.Value;
        set => Mathf.Max(value, 1);
    }
    public virtual int Value
    {
        get => _currentValue2.Value;
        set
        {
            var newValue = Mathf.Clamp(value, 0, MaxValue);
            var prevValue = Value;
            _currentValue2.Value = newValue;
            if (newValue < prevValue) OnCurrentValueDropped();
            else if(newValue > prevValue) OnCurrentValueIncreased();
        }
    }

    public IObservable<int> CurrentValueStream => CurrentValueProperty.StartWith(Value);
    public IObservable<int> MaxValueStream => MaxValueProperty.StartWith(MaxValue);
    public IObservable<(int prevValue, int newValue)> OnValueChanged => _currentValue2.OnSwitched;
    public virtual IReadOnlyReactiveProperty<int> MaxValueProperty => maxValue;
    public virtual IReadOnlyReactiveProperty<int> CurrentValueProperty => _currentValue2;
    
    protected virtual void Awake()
    {
        GetStatManager().AddStat(this);
        _currentValue2.Value = maxValue.Value;
    }

    StatManager GetStatManager()
    {
        if(_statManager != null )return _statManager;
        if(!gameObject.TryGetComponent(out _statManager))
            _statManager = gameObject.AddComponent<StatManager>();
        return _statManager;
    }
    protected void OnDestroy()
    {
        if(_statManager)
            _statManager.RemoveStat(this);
    }

    public StatValues GetValues()
    {
        return new StatValues()
        {
            id = GetStatID(),
            maxValue = MaxValue,
            currentValue = Value
        };
    }
    public void SetValues(StatValues values)
    {
        if (values.id != GetStatID())
        {
            Debug.LogError($"Stat ID mismatch: {values.id} != {GetStatID()}",this);
            return;
        }
        MaxValue = values.maxValue;
        Value = values.currentValue;
    }
    public abstract string GetStatID();


    protected virtual void OnCurrentValueDropped()
    {
        
    }

    protected virtual void OnCurrentValueIncreased()
    {
        
    }
}




public abstract class RegeneratingStat : StatBase, IRegenStatValue
{
    public float regenStartupDelay = 3;

    public abstract RegenConfig GetRegenConfig();
    public virtual bool CanRegen() => true;

    protected override void OnCurrentValueDropped()
    {
        
    }


    public float RegenRate => GetRegenConfig().RegenRate;
    public float RegenResetDelay => GetRegenConfig().RegenResetDelay;
}

public class Regeneration : IDisposable
{
    private readonly IRegenStatValue _regenStatValue;
    private readonly IReadOnlyReactiveProperty<bool> _ableToRegenerate;

    private readonly CompositeDisposable _cd = new();
    private IDisposable _currentResetTimer;
    private IDisposable _currentRegen;

    private Subject<Unit> _onRegen = new();
    private Subject<Unit> _onRegenStartedUp = new();
    private Subject<Unit> _onRegenFinished = new();
    private ReactiveProperty<bool> _isRegenPaused = new();
    private readonly IObservable<Unit> _onRegenStopped;

    public bool isRegenPaused
    {
        set => _isRegenPaused.Value = value;
        get => _isRegenPaused.Value;
    }

    public IObservable<Unit> OnRegenStarted => _onRegenStartedUp;
    public IObservable<Unit> OnRegenStopped => _onRegenStopped;

    public Regeneration(IRegenStatValue regenStatValue, IReadOnlyReactiveProperty<bool> ableToRegenerate)
    {
        _regenStatValue = regenStatValue;
        _ableToRegenerate = ableToRegenerate;
        _onRegenStopped = _onRegenFinished.Merge(_isRegenPaused.Where(t => t).AsUnitObservable(), _ableToRegenerate.Where(t => !t && !isRegenPaused).AsUnitObservable());

        _regenStatValue.OnValueChanged
            .Where(t => t.prevValue < t.newValue)
            .Subscribe(_ => ResetRegen())
            .AddTo(_cd);
        
        _onRegen
            .Where(_ => regenStatValue.Value < regenStatValue.MaxValue)
            .Subscribe(_ => regenStatValue.Value++)
            .AddTo(_cd);
        
        _isRegenPaused.Where(t => !t).AsUnitObservable().Subscribe(_onRegenStartedUp).AddTo(_cd);
        _onRegen.Where(_ => regenStatValue.Value == regenStatValue.MaxValue).AsUnitObservable()
            .Subscribe(_onRegenFinished).AddTo(_cd);
        
        _onRegen.AddTo(_cd);
        _onRegenFinished.AddTo(_cd);
        _onRegenStartedUp.AddTo(_cd);
        if (regenStatValue.Value < regenStatValue.MaxValue) StartRegen();
    }
    public Regeneration(IRegenStatValue regenStatValue)
    {
        _regenStatValue = regenStatValue;
        _onRegenStopped = _onRegenFinished.Merge(_isRegenPaused.Where(t => t).AsUnitObservable());
        
        _regenStatValue.OnValueChanged
            .Where(t => t.prevValue < t.newValue)
            .Subscribe(_ => ResetRegen())
            .AddTo(_cd);
        
        _onRegen.Where(_ => regenStatValue.Value < regenStatValue.MaxValue).Subscribe(_ => regenStatValue.Value++).AddTo(_cd);
        _isRegenPaused.Where(t => !t).AsUnitObservable().Subscribe(_onRegenStartedUp).AddTo(_cd);
        _onRegen.Where(_ => regenStatValue.Value == regenStatValue.MaxValue).AsUnitObservable()
            .Subscribe(_onRegenFinished).AddTo(_cd);
        _onRegen.AddTo(_cd);
        _onRegenFinished.AddTo(_cd);
        _onRegenStartedUp.AddTo(_cd);
        if (regenStatValue.Value < regenStatValue.MaxValue) StartRegen();
    }
    public void ResetRegen()
    {
        if(_currentRegen != null)_currentRegen.Dispose();
        if(_currentResetTimer != null)_currentResetTimer.Dispose();
        isRegenPaused = true;
        _currentResetTimer = Observable.Timer(TimeSpan.FromSeconds(_regenStatValue.RegenResetDelay))
            .Subscribe(_ => StartRegen())
            .AddTo(_cd);
    }

    private void StartRegen()
    {
        isRegenPaused = false;
        if(_currentRegen != null)_currentRegen.Dispose();
        _currentRegen = Observable.Interval(TimeSpan.FromSeconds(_regenStatValue.RegenRate))
            .AsUnitObservable()
            .Subscribe(_onRegen);
    }

    public void Dispose()
    {
        _cd.Dispose();
        _currentRegen?.Dispose();
        _currentResetTimer?.Dispose();
    }
}

[Serializable]
public class RegenConfig
{
    [SuffixLabel("sec"), SerializeField]private float regenResetDelay = 3;
    [SuffixLabel("sec"), SerializeField]private float regenRate = 1;


    public float RegenResetDelay => regenResetDelay;
    public float RegenRate => regenRate;
}


