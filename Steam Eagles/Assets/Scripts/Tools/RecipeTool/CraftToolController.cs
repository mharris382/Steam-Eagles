using Buildings.Rooms;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.RecipeTool
{
    public class CraftToolController : ToolControllerBase
    {
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(CraftToolController)} throw new System.NotImplementedException();");
        }
    }
}