using Buildings;

public interface ILayerSpecificRoomTexSaveLoader : IRoomTilemapTextureSaveLoader
{
    BuildingLayers TargetLayer { get; }
}