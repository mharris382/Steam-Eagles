using Players;
using UI;
using UnityEngine;

namespace Items.UI
{
    public class UIToolWheel : MonoBehaviour
    {
        public Player player;
        public UIWheelBuilder wheelBuilder;



        bool HasResources()
        {
            if(player.CharacterInput==null) return false;
            if(player.CharacterInput.PlayerInput == null) return false;
            
            return true;
        }
        
        
    }
}