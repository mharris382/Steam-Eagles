using System;
using UnityEngine;

public class TilemapEditTrigger : TriggerAreaBase<CharacterTilemapEditor>
{
    
    [SerializeField] private AllowedToEdit characterAllowedToEdit = AllowedToEdit.BOTH;
    
    [Flags]
    private enum AllowedToEdit
    {
        TP = 1,
        BD = 2,
        BOTH = 3,
    }
    
    protected override bool HasTarget(Rigidbody2D rbTarget, out CharacterTilemapEditor value)
    {
        
        throw new NotImplementedException();
    }

    protected override bool HasTarget(Collider2D target, out CharacterTilemapEditor value)
    {
        throw new NotImplementedException();
    }
}