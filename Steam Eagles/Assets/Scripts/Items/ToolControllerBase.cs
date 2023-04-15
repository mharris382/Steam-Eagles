using UnityEngine;

namespace Items
{
    public abstract class ToolControllerBase : MonoBehaviour
    {
        public abstract void OnEquip(Tool tool);
    }
}