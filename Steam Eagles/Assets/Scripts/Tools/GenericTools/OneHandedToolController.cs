using System;
using Buildings.Rooms;
using Tools.BuildTool;
using UnityEngine;

namespace Tools.GenericTools
{
    public class OneHandedToolController : ToolControllerBase
    {
        protected override void OnRoomChanged(Room room)
        {
            Debug.LogWarning($"{nameof(OneHandedToolController)} throw new System.NotImplementedException();");
        }
    }
}