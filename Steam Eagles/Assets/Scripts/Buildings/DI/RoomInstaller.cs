using Buildings.Rooms;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Buildings.DI
{
    public class RoomInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<Room>().FromComponentInHierarchy().AsSingle();
            Container.Bind<RoomTextures>().FromComponentInHierarchy().AsSingle();
            Container.Bind<RoomEvents>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BoundsLookup>().FromNew().AsSingle().NonLazy();
            Container.Bind<ITileColorPicker>().To<PlaceholderTileColorPicker>().AsSingle().NonLazy();
        }

        class PlaceholderTileColorPicker : ITileColorPicker
        {
            public Color GetColorForTile(Vector3Int position, BuildingLayers layer, TileBase tile, Color prevColor)
            {
                return tile == null ? Color.clear : Color.white;
            }
        }
    }
}