using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Players.Shared
{
    [CreateAssetMenu(fileName = "New Game Mode", menuName = "Steam Eagles/New Game Mode", order = 0)]
    public class PlayerGameMode : ScriptableObject
    {
        public InputActionAsset inputActionAsset;
        
        [ValueDropdown(nameof(GetActionMapNames))] public string actionMapName;
        
        
        
        ValueDropdownList<string> GetActionMapNames()
        {
            if(inputActionAsset == null)
                return new ValueDropdownList<string>();
            var list = new ValueDropdownList<string>();
            foreach (var actionMap in inputActionAsset.actionMaps)
            {
                list.Add(actionMap.name, actionMap.name);
            }
            return list;
        }
    }
}