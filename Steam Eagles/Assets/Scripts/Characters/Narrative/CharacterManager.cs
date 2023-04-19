using System;
using System.Collections.Generic;
using CoreLib;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Characters.Narrative
{
    public class CharacterManager : Singleton<CharacterManager>
    {
        public override bool DestroyOnLoad => false;
        
        private Dictionary<string, AsyncOperationHandle<GameObject>> _loadedCharacters 
            = new Dictionary<string, AsyncOperationHandle<GameObject>>();
        
        private Dictionary<string, CharacterDescription> _loadedCharactersDescriptions
            = new Dictionary<string, CharacterDescription>();
        
        
        public event Action<CharacterDescription> OnDescriptionLoaded;
        
        public event Action<GameObject> OnCharacterLoaded;

        public event Action<Character> OnCharacterSpawned;

        public void LoadCharacter(string characterName)
        {
            
        }
        
        public void LoadCharacterDescription(CharacterDescription characterDescription)
        {
            if(_loadedCharactersDescriptions.ContainsKey(characterDescription.name))
                return;
            _loadedCharactersDescriptions.Add(characterDescription.name, characterDescription);
        }

#pragma warning disable CS1998
        public async UniTask<CharacterDescription> LoadCharacterDescriptionAsync(string characterName)
#pragma warning restore CS1998
        {
            if (IsCharacterDescriptionLoaded(characterName))
            {
                return _loadedCharactersDescriptions[characterName];
            }
            else
            {
                var handle = Addressables.LoadAssetAsync<CharacterDescription>(characterName);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var characterDescription = handle.Result;
                    _loadedCharactersDescriptions.Add(characterName, characterDescription);
                    OnDescriptionLoaded?.Invoke(characterDescription);
                    return characterDescription;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
#pragma warning disable CS1998
        public async UniTask<Character> LoadCharacterAsync(string characterName)
#pragma warning restore CS1998
        {
            CharacterDescription characterDescription;
            string key = $"{characterName}_Prefab";
            var result = Addressables.LoadAssetAsync<GameObject>(key);
            await result.Task;
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                var character = result.Result.GetComponent<Character>();
                _loadedCharacters.Add(characterName, result);
                OnCharacterLoaded?.Invoke(character.gameObject);
                return character;
            }
            else
            {
                Debug.LogError($"LoadCharacterAsync {characterName} failed. Expected key {key}");
                throw new Exception();
            }
            // if (IsCharacterLoaded(characterName))
            // {
            //     return _loadedCharacters[characterName].Result.GetComponent<Character>();
            // }
            //
            // if(!IsCharacterDescriptionLoaded(characterName))
            // {
            //     characterDescription = await LoadCharacterDescriptionAsync(characterName);
            // }
            // else
            // {
            //     characterDescription = _loadedCharactersDescriptions[characterName];
            // }
            // var prefabHandle = characterDescription.characterComponentReference.InstantiateAsync();
            // await prefabHandle.Task;
            //
            // if (prefabHandle.Status == AsyncOperationStatus.Succeeded)
            // {
            //     var character = prefabHandle.Result;
            //     _loadedCharacters.Add(characterName, prefabHandle);
            //     OnCharacterLoaded?.Invoke(character.gameObject);
            //     return character;
            // }

            throw new Exception();
        }
        public void LoadCharacter(CharacterDescription characterDescription)
        {
            if (_loadedCharactersDescriptions.ContainsKey(characterDescription.name))
            {
                return;
            }
            _loadedCharactersDescriptions.Add(characterDescription.name, characterDescription);
        }
        
        public bool IsCharacterLoaded(CharacterDescription characterDescription)
        {
            return IsCharacterLoaded(characterDescription.name);
        }
        
        
        private void LoadCharacterDescription(string characterName, [CanBeNull] Action<CharacterDescription> onLoaded)
        {
            Addressables.LoadAssetAsync<CharacterDescription>(characterName).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    var characterDescription = handle.Result;
                    _loadedCharactersDescriptions.Add(characterName, characterDescription);
                    onLoaded?.Invoke(_loadedCharactersDescriptions[characterName]);
                }
                else
                {
                    throw new Exception();
                }
            };
        }
        
        private void LoadCharacter(string characterName, Action<Character> prefabLoaded)
        {
            if (IsCharacterLoaded(characterName))
            {
                prefabLoaded?.Invoke(_loadedCharacters[characterName].Result.GetComponent<Character>());
                return;
            }

            string addressableKey = $"{characterName}_Prefab";
            var loadOp = Addressables.LoadAssetAsync<GameObject>(addressableKey);
            _loadedCharacters.Add(characterName, loadOp);
            loadOp.Completed += res =>
            {
                prefabLoaded?.Invoke(res.Result.GetComponent<Character>());
            };
        }
        public Character GetCharacter(string characterName)
        {
            if (!IsCharacterLoaded(characterName))
            {
                return null;
            }
            return _loadedCharacters[characterName].Result.GetComponent<Character>();
        }

        public CharacterDescription GetCharacterDescription(string characterName)
        {
            if (!IsCharacterDescriptionLoaded(characterName))
            {
                return null;
            }

            if (_loadedCharactersDescriptions[characterName] == null)
            {
                _loadedCharactersDescriptions.Remove(characterName);
                return null;
            }

            return _loadedCharactersDescriptions[characterName];
        }
        
        public bool IsCharacterLoaded(string characterName)
        {
            if (_loadedCharacters.ContainsKey(characterName) && _loadedCharacters[characterName].IsDone)
            {
                return _loadedCharacters[characterName].IsDone;
            }

            return false;
        }
        
        public bool IsCharacterDescriptionLoaded(string characterName)
        {
            if(_loadedCharactersDescriptions.ContainsKey(characterName))
                return _loadedCharactersDescriptions[characterName] != null;
            
            return false;
        }
        
    }
}