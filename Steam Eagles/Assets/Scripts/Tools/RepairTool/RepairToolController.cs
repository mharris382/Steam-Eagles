using Buildings.Rooms;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.RepairTool
{
    public class RepairToolController : ToolControllerBase
    {
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(RepairToolController)} throw new System.NotImplementedException();");
        }
    }
}