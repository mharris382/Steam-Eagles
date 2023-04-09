using UnityEngine;
using UnityEngine.InputSystem;

namespace InputIcons
{
    //attached to a PlayerInput component, this will report device changes to the InputIconsManagerSO
    [RequireComponent(typeof(PlayerInput))]
    public class InputIconsDeviceChangeDetection : MonoBehaviour
    {
        void OnControlsChanged()
        {
            //InputIconsManagerSO.HandleControlsChanged(GetComponent<PlayerInput>());
        }
    }
}

