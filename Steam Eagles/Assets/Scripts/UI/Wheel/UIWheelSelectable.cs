using System;
using UnityEngine;

namespace UI.Wheel
{
    public struct UIWheelSelectable
    {
        public string name;
        public bool isLocked;
        public Sprite sprite;
        public Action onSelected;
    }
}