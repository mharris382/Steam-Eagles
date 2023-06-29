using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;


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
        get => Value;
        set => Value = value;
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