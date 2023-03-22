using System.Collections.Generic;
using UnityEngine;

namespace Players
{
    [System.Serializable]
    public class PlayerCharacterAssignments
    {
        public List<Assignment> assignments = new List<Assignment>();
        
        [System.Serializable]
        public class Assignment
        {
            public int playerNumber;
            public string characterName;
        }
    }
}