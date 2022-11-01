using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Img =  UnityEngine.UI.Image;
namespace UI
{
    public abstract class UISelectedAbilitySlot : MonoBehaviour
    {
        public Img Image => _image == null ? (_image = GetComponent<Img>()) : _image;
        private Img _image;
        
        
    }
}