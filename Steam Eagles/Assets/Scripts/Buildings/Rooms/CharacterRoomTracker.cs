using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    public class CharacterRoomTracker : MonoBehaviour
    {
        [ValueDropdown(nameof(GetValidCharacters))]
        public string characterName = "Transporter";


        private void Awake()
        {
            
        }

        ValueDropdownList<string> GetValidCharacters()
        {
            var list = new ValueDropdownList<string>();
            list.Add("Transporter");
            list.Add("Builder");
            return list;
        }
    }
}