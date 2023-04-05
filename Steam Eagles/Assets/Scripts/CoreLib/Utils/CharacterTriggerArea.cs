using System;
using UnityEngine;
using UnityEngine.Events;
using static CharacterTriggerArea.CharactersInZone;

public class CharacterTriggerArea : TriggerAreaBase<GameObject>
{
    [Flags]
    public enum CharactersInZone
    {
        NONE = 0,
        TRANSPORTER =0b01,
        BUILDER = 0b10
    }
    
    public UnityEvent<CharactersInZone> onCharactersInAreaChanged;

    public UnityEvent onAnyCharacterInArea;
    public UnityEvent onAllCharacterLeftArea;
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

    private int cnt = 0;
    private void Start()
    {
        onCharactersInAreaChanged?.Invoke(_charactersInArea);
        onCharactersInAreaChanged.AddListener(charactersInZone =>
        {
            if (charactersInZone == NONE)
            {
                onAllCharacterLeftArea?.Invoke();
                cnt = 0;
            }
            else
            {
                if(cnt == 0)
                    onAnyCharacterInArea?.Invoke();
                cnt = (charactersInZone & TRANSPORTER) == TRANSPORTER ? 1 : 0;
                cnt += (charactersInZone & BUILDER) == BUILDER ? 1 : 0;
            }
        });
}

    protected override void OnTargetAdded(GameObject target, int totalNumberOfTargets)
    {
        if (target.CompareTag("Builder"))
        {
            CharactersInsideArea |= BUILDER;
        }
        else if (target.CompareTag("Transporter"))
        {
            CharactersInsideArea |= TRANSPORTER;
        }
        base.OnTargetAdded(target, totalNumberOfTargets);
    }

    protected override void OnTargetRemoved(GameObject target, int totalNumberOfTargets)
    {
        if (target.CompareTag("Builder"))
        {
            CharactersInsideArea &= ~BUILDER;
        }
        else if (target.CompareTag("Transporter"))
        {
            CharactersInsideArea &= ~TRANSPORTER;
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