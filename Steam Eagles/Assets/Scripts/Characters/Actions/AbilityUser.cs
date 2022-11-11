using System;
using StateMachine;
using UnityEngine;

/// <summary>
/// since the ability system is getting newer more complex features added, I think it's important to setup a shared data container to hold shared state information
/// and act as a dependency injection entry point for other systems to interface with.
/// </summary>
public class AbilityUser : MonoBehaviour
{
    
    [Serializable]
    public class BlockInventory
    {
        public bool infiniteResources;
        public int initialPipes = 10;
        public int initialBlocks = 10;
    
        int _storedPipes;
        public int StoredPipes
        {
            get {
                if(infiniteResources)return int.MaxValue;
                return _storedPipes;
            }
            set => _storedPipes =value;
        }

        int _storedBlocks;
        public int StoredBlocks
        {
            get {
                if(infiniteResources)return int.MaxValue;
                return _storedBlocks;
            }
            set => _storedBlocks =value;
        }

        public void Awake()
        {
            _storedBlocks = initialBlocks;
            _storedPipes = initialPipes;
        }
    }

    [SerializeField]  private CharacterState characterState;
    [SerializeField] private SharedTransform characterTransform;
    public BlockInventory blockInventory = new BlockInventory();

   
    
    private void Awake()
    {
       blockInventory.Awake();
       if (characterTransform != null)
       {
           if (characterTransform.HasValue) TryGetStateFromTransform(characterTransform.Value);
           characterTransform.onValueChanged.AddListener(TryGetStateFromTransform);
           
           void TryGetStateFromTransform(Transform t)
           {
               var characterState = t.GetComponent<CharacterState>();
               if (characterState != null)
               {
                   this.characterState = characterState;
                   Debug.Log($"Ability user found character state {characterState.name}");
               }
           }
       }

       if (characterState == null) characterState = GetComponentInParent<CharacterState>();
    }
}