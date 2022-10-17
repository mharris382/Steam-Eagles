using System;
using CoreLib;
using StateMachine;
using UnityEngine;
using World;

public class CharacterTilemapEditor : MonoBehaviour
{
    public SharedTilemap editableTilemap;
    
    public SharedCamera characterCamera;

    public SharedTransform otherCharacter;
    public SpriteRenderer previewObj;
    
    
    public Color validColor = Color.green;
    public Color invalidColor = Color.red;
    public Color outOfBounds = Color.gray;
    public LayerMask editTriggerLayer;
    [SerializeField]
    private ScreenTarget screenTarget;

    private Rect screenRect;

    private enum ScreenTarget
    {
        LEFT,
        RIGHT,
        FULL
    }
    private void Awake()
    {
        editableTilemap.Value = null;
    }

    // private void Update()
    // {
    //     if (!editableTilemap.HasValue || !characterCamera.HasValue)
    //     {
    //         previewObj.gameObject.SetActive(false);
    //         return;
    //     }
    //
    //     this.screenRect = new Rect(0,0, Screen.width, Screen.height);
    //
    //     switch (screenTarget)
    //     {
    //         case ScreenTarget.LEFT:
    //             if (Input.mousePosition.x > Screen.width / 2f)
    //             {
    //                 DisableEditor();
    //                 return;
    //             }
    //
    //             screenRect.center = new Vector2(0, 0);
    //             screenRect.width = Screen.width / 2f;
    //             break;
    //         case ScreenTarget.RIGHT:
    //             if (Input.mousePosition.x < Screen.width / 2f)
    //             {
    //                 DisableEditor();
    //                 return;
    //             }
    //
    //             screenRect.width = Screen.width / 2f;
    //             break;
    //     }
    //
    //     var mp = characterCamera.Value.ScreenToWorldPoint(Input.mousePosition);
    //     if (otherCharacter.HasValue)
    //     {
    //         mp -= otherCharacter.Value.position;
    //     }
    //
    //     EnableEditor();
    //
    //     var cp = editableTilemap.Value.WorldToCell(mp);
    //     var pos =editableTilemap.Value.GetCellCenterWorld(cp);
    //     
    //     var coll = Physics2D.OverlapPoint(pos, editTriggerLayer);
    //     pos.z = 0;
    //     previewObj.transform.position = pos + (Vector3.forward * 5);
    //     
    //    
    //
    //     if (coll != null)
    //     {
    //         previewObj.color = editableTilemap.Value.HasTile(cp) ? invalidColor : validColor;
    //     }
    //     else
    //     {
    //         previewObj.color = outOfBounds;
    //     }
    //
    // }
    //
    // private void EnableEditor()
    // {
    //     if (!previewObj.gameObject.activeSelf)
    //         previewObj.gameObject.SetActive(true);
    //     Cursor.visible = false;
    // }
    //
    // private void DisableEditor()
    // {
    //     if (previewObj.gameObject.activeSelf)
    //         previewObj.gameObject.SetActive(false);
    //     Cursor.visible = true;
    // }

    private void OnDrawGizmos()
    {
    }
}