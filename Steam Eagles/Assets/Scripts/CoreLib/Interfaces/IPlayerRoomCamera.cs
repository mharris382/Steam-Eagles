using UnityEngine;

namespace CoreLib.Interfaces
{
    public interface IPlayerRoomCamera
    {
        void SetRoom(RoomDescription roomDescription);
        void SetCameraActive(Transform subjectTransform, bool isActive);
    }


    public struct RoomDescription
    {
        private readonly Vector2Int _roomGridSize;
        private readonly Component _roomComponent;
        private readonly Component _buildingComponent;

        public RoomDescription(Vector2Int roomGridSize, Component roomComponent, Component buildingComponent)
        {
            _roomGridSize = roomGridSize;
            _roomComponent = roomComponent;
            _buildingComponent = buildingComponent;
        }

        public T GetRoomComponent<T>() where T : Component => (T)_roomComponent;

        public T GetBuildingComponent<T>() where T : Component => (T)_buildingComponent;
    }
}