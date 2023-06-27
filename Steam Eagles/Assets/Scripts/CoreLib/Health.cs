using System;
using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;


public abstract class StatBase : MonoBehaviour
{
    [SerializeField] private IntReactiveProperty maxValue = new IntReactiveProperty(10);
    
    private IntReactiveProperty _currentValue = new IntReactiveProperty(10);
    private StatManager _statManager;

    public virtual int MaxValue
    {
        get => maxValue.Value;
        set => Mathf.Max(value, 1);
    }

    public virtual int CurrentValue
    {
        get => _currentValue.Value;
        set => _currentValue.Value = Mathf.Clamp(value, 0, MaxValue);
    }

    public IObservable<int> CurrentValueStream => CurrentValueProperty.StartWith(CurrentValue);
    public IObservable<int> MaxValueStream => MaxValueProperty.StartWith(MaxValue);

    public virtual IReadOnlyReactiveProperty<int> MaxValueProperty => maxValue;
    public virtual IReadOnlyReactiveProperty<int> CurrentValueProperty => _currentValue;
    
    protected virtual void Awake()
    {
        _currentValue.Value = maxValue.Value;
        if(!gameObject.TryGetComponent(out _statManager))
            _statManager = gameObject.AddComponent<StatManager>();
        _statManager.AddStat(this);
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
            currentValue = CurrentValue
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
        CurrentValue = values.currentValue;
    }
    public abstract string GetStatID();
}

[DisallowMultipleComponent]
public class Health : StatBase
{
     [FoldoutGroup("Events",false)] public UnityEvent<Transform> onDeath;
     [FoldoutGroup("Events",false)] public UnityEvent onRespawn;
    
    private ReadOnlyReactiveProperty<bool> _isDeadProperty;
    private Transform _deathPoint;
    
    public bool IsDead => _isDeadProperty == null ? true : _isDeadProperty.Value;
    
    public int MaxHealth
    {
        get => MaxValue;
        set => MaxValue = value;
    }

    public int CurrentHealth
    {
        get => CurrentValue;
        set => CurrentValue = value;
    }
    public IReadOnlyReactiveProperty<int> CurrentHealthStream => CurrentValueProperty;
    public IReadOnlyReactiveProperty<int> MaxHealthStream => MaxValueProperty;

    protected override void Awake()
    {
        base.Awake();
        _isDeadProperty = CurrentHealthStream.Select(t => t == 0).ToReadOnlyReactiveProperty();
        _isDeadProperty.Where(t => t).Subscribe(_ =>
        {
            _deathPoint = new GameObject("DeathPoint").transform;
            _deathPoint.transform.position = transform.position;
            onDeath?.Invoke(_deathPoint);
        }).AddTo(this);
    }

    public void Respawn()
    {
        CurrentHealth = MaxHealth;
        onRespawn?.Invoke();
    }

    public override string GetStatID() => "Health";
}