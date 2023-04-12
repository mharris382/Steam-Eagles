using System;
using CoreLib;
using CoreLib.SharedVariables;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Characters
{
    [Serializable]
    public class CharacterAssignment
    {
        
        [TabGroup("Character")][GUIColor(nameof(GetGuiColor))]public string characterName;
        [TabGroup("Character")][GUIColor(nameof(GetGuiColor))][SerializeField] Character prefab;
            
        
        [TabGroup("Spawn Point")][Obsolete("Use SpawnDatabase instead")]
        public SharedTransform spawnPoint;
        
        [TabGroup("Spawn Point")][Obsolete("Use SpawnDatabase instead")] 
        public Vector3 defaultPosition;
            
        
        [TabGroup("Debugging")]    public Color characterColor = Color.red;
        [TabGroup("Debugging")]    public bool hideTransformInEditor = false;

        private Color GetGuiColor() => characterColor.Lighten(.5f);

        private CharacterInputState _spawnedCharacter;
            
            
            
        public Vector3 SpawnPosition => SpawnTransform.position;

            
            
            
        /// <summary>
        /// used to move the character's spawn point in the world
        /// </summary>
        public Transform SpawnTransform
        {
            get
            {
                if (spawnPoint == null) spawnPoint = ScriptableObject.CreateInstance<SharedTransform>();
                if (!spawnPoint.HasValue)
                {
                    spawnPoint.Value = new GameObject($"{characterName} Spawn Transform").transform;
                    if(hideTransformInEditor)spawnPoint.Value.hideFlags = HideFlags.HideInHierarchy;
                    spawnPoint.Value.position = defaultPosition;
                    spawnPoint.name = $"{characterName} Spawn Transform";
                }
                return spawnPoint.Value;
            }
        }

        public CharacterInputState InstantiateCharacter()
        {
            if (_spawnedCharacter != null) return _spawnedCharacter;
            var character = Object.Instantiate(prefab, SpawnPosition, Quaternion.identity);
            character.name = characterName;
            CharacterInputState characterInputState;
            if (!character.gameObject.TryGetComponent(out characterInputState))
            {
                characterInputState = character.gameObject.AddComponent<CharacterInputState>();
            }
            _spawnedCharacter = characterInputState;
            return characterInputState;
        }
        
            
        public void DestroyCharacter(Character character)
        {
            Object.Destroy(character.gameObject);
        }
            
        public void MoveCharacterToSpawn(Character character) => character.transform.position = SpawnPosition;

        public void ResetSpawnPosition()
        {
            SpawnTransform.position = defaultPosition;
        }
            
        public void OnDrawGizmos()
        {
            Gizmos.color = characterColor;
            Gizmos.DrawSphere(defaultPosition, 0.125f);
        }
    }
}