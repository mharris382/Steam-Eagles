using System.Collections;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Utilities;

public enum DamageType
{
    STANDARD,
    FIRE,
    SUFFOCATION,
    POISON,
    SHOCK,
    PRESSURIZED_STEAM
}



[System.Serializable]
public class DamageConfig
{
    public string name;
    public DamageType type;
    public DamageLogic logic;
    public DamageFeedbacks feedbacks;
    
    
    [System.Serializable]
    public class DamageFeedbacks
    {
        public GameFX impactFX;
        public GameFX deathFX;
    }
    
    [System.Serializable]
    public class DamageLogic
    {
        public int damageAmount = 1;
        public int knockbackAmount = 1;
        
    }
}

public class GlobalHealthConfig
{
    

}

public static class HealthExtensions
{
    private static LayerMask _damageableLayers;
    static HealthExtensions()
    {
        _damageableLayers =LayerMask.GetMask("Balloons", "Ground", "Players");
    }
    
    public static LayerMask GetDamageableLayers()
    {
        return _damageableLayers;
    }
    
    public static bool Damage(this GameObject gameObject, int amount = 1)
    {
        if (gameObject == null) return false;
        if(gameObject.TryGetComponent<Health>(out var health) && !health.IsDead)
        {
            health.CurrentHealth -= amount;
            return true;
        }
        return false;
    }

    public static bool Damage(this Collider2D collider2D)
    {
        if (collider2D == null) return false;
        Rigidbody2D attachedRigidbody;
        return Damage(collider2D.gameObject) || ((attachedRigidbody = collider2D.attachedRigidbody) != null && Damage(attachedRigidbody.gameObject));
    }
    
    public static bool Damage(this RaycastHit2D raycastHit2D, int amount)
    {
        if (!raycastHit2D) return false;
        return raycastHit2D.Damage(amount);
    }
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