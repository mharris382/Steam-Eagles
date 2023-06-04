using System;
using UnityEngine;

namespace Buildings.Rooms
{
    [Serializable]
    public class RoomCameraConfig
    {
        [SerializeField]
        private bool followPlayer;
        public bool followPlayerX => followPlayer && (((int)followAxes & (int)FollowAxes.X) != 0);
        public bool followPlayerY => followPlayer && ((int)followAxes & (int)FollowAxes.Y) != 0;
        public bool confineToRoom = true;
        public FollowAxes followAxes = FollowAxes.BOTH;
        public Rect padding;
        
        [Flags]
        public enum FollowAxes
        {
            NONE,
            X, Y,
            BOTH = X | Y
        }


        public bool IsDynamic
        {
            get => followPlayer;
            set => followPlayer = value;
        }
        public Vector3 GetCameraPosition(Room room, Vector3 characterPositionWs,
            Vector3 cameraPositionWs)
        {
            if (followPlayer == false)
                return cameraPositionWs;
            if(followAxes == FollowAxes.NONE)
                return cameraPositionWs;
           
            
            if (confineToRoom)
            {
                var wsBounds = room.WorldSpaceBounds;
                wsBounds.center += new Vector3(padding.x, padding.y);
                wsBounds.size += new Vector3(padding.width, padding.height);
                if (wsBounds.Contains(characterPositionWs))
                {
                    characterPositionWs = wsBounds.ClosestPoint(characterPositionWs);
                }
            }
            if (followPlayerX)
            {
                cameraPositionWs.x = characterPositionWs.x;
            }

            if (followPlayerY)
            {
                cameraPositionWs.y = characterPositionWs.y;
            }
            Debug.Log($"Following player position: {characterPositionWs}, camera position: {cameraPositionWs}" +
                      $" room name: {room.name}", room);
            return cameraPositionWs;
        }
    }
}