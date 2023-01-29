using System;
using System.Collections.Generic;
using Characters.Actions;
using UnityEngine;

[Obsolete("using new input system")]
public class AbilityButtonInput : MonoBehaviour
{
    private CharacterState _characterState;

    public AbilityController downAbility;
    public AbilityController upAbility;
    public AbilityController leftAbility;
    public AbilityController rightAbility;

    private Vector2 lastVelocity;
    
    public bool useLegacyInput = true;
    public KeyCode abilityKey = KeyCode.E;
    public float cooldownTime = 0.25f;
    private bool _inCooldown =false;

    private void Awake()
    {
        _characterState = GetComponentInParent<CharacterState>();
    }

    void ResetCooldown()
    {
        this._inCooldown = false;
    }

    private void Update()
    {
        if (useLegacyInput)
        {
            if (Input.GetKey(abilityKey))
            {
                TryAbility();
            }
        }
    }

    private void TryAbility()
    {
        if (_inCooldown) return;
        foreach (var ability in GetAbilityCheckOrder())
        {
            
        }
    }

    IEnumerable<AbilityController> GetAbilityCheckOrder()
    {
        var vel = new Vector2(
            Mathf.Abs(_characterState.VelocityX > 0.1f ? _characterState.VelocityX : lastVelocity.x),
            Mathf.Abs(_characterState.VelocityY> 0.1f ? _characterState.VelocityY : lastVelocity.y));
        lastVelocity = vel;
        
        var move = new Vector2(_characterState.MoveX, _characterState.MoveY);
        
        var moveAim = new Vector2(
            Mathf.Abs(_characterState.MoveX) > 0.1f ? move.x : vel.x,
            Mathf.Abs(_characterState.MoveY) > 0.1f ? move.y : vel.y);
        
        bool checkHorizontal = Mathf.Abs(moveAim.x) > 0;
        bool checkVertical = Mathf.Abs(moveAim.y) > 0;
        if (checkVertical)
        {
            yield return (lastVelocity.y > 0) ? upAbility : downAbility;
            yield return (lastVelocity.y > 0) ? downAbility : upAbility;
        }
        else
        {
            yield return upAbility;
            yield return downAbility;
        }
        if (checkHorizontal)
        {
            yield return (lastVelocity.x > 0) ? rightAbility : leftAbility;
            yield return (lastVelocity.x > 0) ? leftAbility : rightAbility;
        }
        else
        {
            yield return rightAbility;
            yield return leftAbility;
        }

      
    }
    
}