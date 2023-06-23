using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public UnityEvent<Transform> onDeath;
    public UnityEvent onRespawn;
    
    [SerializeField] int maxHealth = 100;
    private IntReactiveProperty _currentHealth = new();
    private ReadOnlyReactiveProperty<bool> _isDeadProperty;
    private Transform _deathPoint;
    private void Awake()
    {
        _currentHealth.Value = maxHealth;
        _isDeadProperty = _currentHealth.Select(t => t == 0).ToReadOnlyReactiveProperty();
        _isDeadProperty.Where(t => t).Subscribe(_ =>
        {
            _deathPoint = new GameObject("DeathPoint").transform;
            _deathPoint.transform.position = transform.position;
            onDeath?.Invoke(_deathPoint);
        }).AddTo(this);
    }

    public bool IsDead => _isDeadProperty == null ? true : _isDeadProperty.Value;
    public int MaxHealth => maxHealth;
    public int CurrentHealth
    {
        get => _currentHealth.Value;
        set => _currentHealth.Value = Mathf.Clamp(value, 0, MaxHealth);
    }

    public void Respawn()
    {
        _currentHealth.Value = MaxHealth;
        onRespawn?.Invoke();
    }
}