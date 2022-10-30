using System;
using UnityEngine;
using UnityEngine.Events;

public class CharacterTriggerArea : TriggerAreaBase<GameObject>
{
    [Flags]
    public enum CharactersInZone
    {
        NONE = 0,
        TRANSPORTER =0b01<<1,
        BUILDER = 0b01<<2
    }
    
    public UnityEvent<CharactersInZone> onCharactersInAreaChanged;


    private CharactersInZone _charactersInArea;

    private CharactersInZone CharactersInsideArea
    {
        get
        {
            return _charactersInArea;
        }
        set
        {
           
                _charactersInArea = value;
                onCharactersInAreaChanged?.Invoke(value);
            
        }
    }

    private void Start()
    {
        onCharactersInAreaChanged?.Invoke(_charactersInArea);
    }

    protected override void OnTargetAdded(GameObject target, int totalNumberOfTargets)
    {
        if (target.CompareTag("Builder"))
        {
            CharactersInsideArea |= CharactersInZone.BUILDER;
        }
        else if (target.CompareTag("Transporter"))
        {
            CharactersInsideArea |= CharactersInZone.TRANSPORTER;
        }
        base.OnTargetAdded(target, totalNumberOfTargets);
    }

    protected override void OnTargetRemoved(GameObject target, int totalNumberOfTargets)
    {
        if (target.CompareTag("Builder"))
        {
            CharactersInsideArea &= ~CharactersInZone.BUILDER;
        }
        else if (target.CompareTag("Transporter"))
        {
            CharactersInsideArea &= ~CharactersInZone.TRANSPORTER;
        }
        
        base.OnTargetRemoved(target, totalNumberOfTargets);
    }

    protected override bool HasTarget(Rigidbody2D rbTarget, out GameObject value)
    {
        if (rbTarget.gameObject.CompareTag("Transporter") || rbTarget.gameObject.CompareTag("Builder"))
        {
            value = rbTarget.gameObject;
            return true;
        }

        value = null;
        return false;
    }

    protected override bool HasTarget(Collider2D target, out GameObject value)
    {
        value = null;
        return false;
    }
}