using System;
using System.Collections;
using Buildings;
using Buildings.Messages;
using Buildings.Rooms;
using Buildings.Rooms.Tracking;
using Items;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class BuildToolController : ToolControllerBase
    {
        public TilePathTool pathTool;


        protected override void OnRoomChanged(Room room)
        {
            if (room == null)
            {
                pathTool.enabled = false;
                return;
            }
            pathTool.enabled = room.buildLevel == BuildLevel.FULL;
            HasRoom = room.buildLevel == BuildLevel.FULL;
        }
    }
}