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
            if(player.InputWrapper==null) return false;
            if(player.InputWrapper.PlayerInput == null) return false;
            
            return true;
        }
        
        
    }
}