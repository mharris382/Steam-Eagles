using System;
using UnityEngine;

namespace Players
{
    /// <summary>
    /// represents the binding of a particular player (input mapping)
    /// to a specific character
    /// </summary>
    public class PlayerCharacterBinding : IDisposable
    {
        public int playerNumber;
        public string characterName;

        public bool IsDisposed 
        { 
            get;
            private set;
        }
        
        public PlayerCharacterBinding(int playerNumber, string characterName)
        {
            this.playerNumber = playerNumber;
            this.characterName = characterName;
            IsDisposed = false;
        }


        /// <summary>
        /// unbinds the character from the player
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            //TODO: raise event
        }
    }
}