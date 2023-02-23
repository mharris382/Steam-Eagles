using System;
using UnityEngine;

namespace UI
{
    public struct UIWheelSelectable
    {
        public string name;
        public bool isLocked;
        public Sprite sprite;
        public Action onSelected;
    }
}