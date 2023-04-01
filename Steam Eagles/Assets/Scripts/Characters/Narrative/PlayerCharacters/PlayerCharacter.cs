
using UnityEngine;


namespace Characters.Narrative.PlayerCharacters
{
    public class PlayerCharacter : Character
    {
        /// <summary>
        /// player characters are currently tagged with their name
        /// </summary>
        public override string CharacterName => tag;
    }
}