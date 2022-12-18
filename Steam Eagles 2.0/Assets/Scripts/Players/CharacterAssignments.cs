using System;
using UnityEngine;

namespace Players
{
    [Serializable]
    public class CharacterAssignments
    {
        public PlayableCharacter[] playableCharacters = new PlayableCharacter[2]{
            new PlayableCharacter(){characterName = "Builder", characterColor = Color.cyan },
            new PlayableCharacter(){characterName = "Transporter", characterColor = Color.yellow }
        };
    
    }



    [Serializable]
    public class PlayableCharacter
    {
        public string characterName;
        public GameObject characterPrefab;
        public Color characterColor;
    }
}