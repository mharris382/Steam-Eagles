using System;
using CoreLib;
using StateMachine;
using UniRx;
using UnityEngine;
using World;




public class CharacterTilemapEditor : MonoBehaviour
{
    public SharedTilemap editableTilemap;

    [SerializeField] private SharedInputs sharedInputs;

    

    [Serializable]
   private class SharedInputs
    {
        [SerializeField] internal SharedBool characterCanEdit;
        [SerializeField] internal SharedBool characterPreviewing;
        [SerializeField] internal SharedBool executingCharacterAction;
        
        public bool CanCharacterEdit() => characterCanEdit == null || characterCanEdit.Value;
        public bool IsCharacterPreviewing() => 
            characterPreviewing == null || characterPreviewing.Value;

        public bool ExecutingCharacterAction() =>
            executingCharacterAction != null && executingCharacterAction.Value;

        
    }

  
    

    private void Awake()
    {
        editableTilemap.Value = null;
    }

    public void Update()
    {
        if(!sharedInputs.characterCanEdit.Value)
        {
            DisableEditing();
            return;
        }

        EnableEditingUI();
    }

    private void EnableEditingUI()
    {
        
    }

    private void DisableEditing()
    {
        
    }


    private void OnDrawGizmos()
    {
    }
}