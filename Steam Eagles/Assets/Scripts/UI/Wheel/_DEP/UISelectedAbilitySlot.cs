using UnityEngine;
using Img =  UnityEngine.UI.Image;
namespace UI.Wheel
{
    [System.Obsolete("Use UIWheelSelectable instead")]
    public abstract class UISelectedAbilitySlot : MonoBehaviour
    {
        public Img Image => _image == null ? (_image = GetComponent<Img>()) : _image;
        private Img _image;
        
        
    }
}