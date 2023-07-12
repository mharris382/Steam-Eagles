using CoreLib.Interfaces;
using UnityEngine;

namespace Items
{
    public class DummyToolHider : MonoBehaviour, IHideTools
    {
        public bool ToolsHidden { get; set; }
    }
}