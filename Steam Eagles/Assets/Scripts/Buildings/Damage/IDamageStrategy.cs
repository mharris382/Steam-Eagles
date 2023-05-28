using System;
using System.Collections.Generic;
using Buildings.Rooms;
using UnityEngine;
using Zenject;
using System.Linq;
using Buildings.TileGrids;
using Object = UnityEngine.Object;

namespace Buildings.Damage
{
    public class RoomDamageOptionProvider : IDamageOptionProvider
    {
        
        public class Factory : PlaceholderFactory<Room,IDamageOptionProvider>{}
        private readonly Room _room;
        private readonly IBuildingRoomLookup _roomCellLookup;
        private readonly IBuildingTilemaps _tilemaps;

        public RoomDamageOptionProvider(Room room, IBuildingRoomLookup roomCellLookup, IBuildingTilemaps tilemaps)
        {
            _room = room;
            _roomCellLookup = roomCellLookup;
            _tilemaps = tilemaps;
        }
        public IEnumerable<Vector3Int> GetOptionsForLayer(BuildingLayers layers)
        {
            var cells = _roomCellLookup.GetCellsForRoom(_room, layers);
            for (int i = 0; i < cells.x; i++)
            {
                for (int j = 0; j < cells.y; j++)
                {
                    var cell = new Vector3Int(i, j);
                    if(_tilemaps.HasTile((Vector2Int)cell, layers))
                    {
                        yield return cell;
                    }
                }
            }
        }
        public IEnumerable<BoundsInt> GetDamageAreasForLayer(BuildingLayers layers)
        {
            yield return _roomCellLookup.GetCellsForRoom(_room, layers);
        }
    }


    public class BuildingOptionProvider : IDamageOptionProvider, IInitializable
    {
        private readonly RoomDamageOptionProvider.Factory _roomDamageOptionProviderFactory;
        private readonly Dictionary<BuildingLayers, IDamageOptionProvider[]> _damageOptionProviders = new Dictionary<BuildingLayers, IDamageOptionProvider[]>();
        private Building _building;
        private bool _inited;
        public BuildingOptionProvider(Building building, RoomDamageOptionProvider.Factory roomDamageOptionProviderFactory)
        {
            _building = building;
            _roomDamageOptionProviderFactory = roomDamageOptionProviderFactory;
        }
        public IEnumerable<Vector3Int> GetOptionsForLayer(BuildingLayers layers)
        {
            if (!_inited)
            {
                Debug.LogError("Not initialized");
                yield break;
            }

            if (!_damageOptionProviders.ContainsKey(layers))
            {
                Debug.LogError($"{layers} is NOT DESTROYABLE");
                throw new InvalidOperationException();
            }
            var roomOptionProviders = _damageOptionProviders[layers];
            foreach (var roomOptionProvider in roomOptionProviders)
            {
                foreach (var option in roomOptionProvider.GetOptionsForLayer(layers))
                {
                    yield return option;
                }
            }
        }

        public IEnumerable<BoundsInt> GetDamageAreasForLayer(BuildingLayers layers)
        {
            if (!_inited)
            {
                Debug.LogError("Not initialized");
                yield break;
            }

            if (!_damageOptionProviders.ContainsKey(layers))
            {
                Debug.LogError($"{layers} is NOT DESTROYABLE");
                throw new InvalidOperationException();
            }
            var roomOptionProviders = _damageOptionProviders[layers];
            foreach (var roomOptionProvider in roomOptionProviders)
            {
                foreach (var boundsInt in roomOptionProvider.GetDamageAreasForLayer(layers))
                    yield return boundsInt;
            }
        }

        public void Initialize()
        {
            var damageableRooms = _building.Rooms.AllRooms.Where(t => t.IsDamageable).ToArray();
            Debug.Log($"Found {damageableRooms.Length} damageable rooms!");
            foreach (var savedLayer in BuildingUtils.SavedLayers)
            {
                var roomOptionProviders = new IDamageOptionProvider[damageableRooms.Length];
                for (int i = 0; i < damageableRooms.Length; i++)
                {
                    roomOptionProviders[i] = _roomDamageOptionProviderFactory.Create(damageableRooms[i]);
                }
                _damageOptionProviders[savedLayer] = roomOptionProviders;
            }
            _inited = true;
        }
    }
    
    public interface IDamageOptionProvider
    {
        IEnumerable<Vector3Int> GetOptionsForLayer(BuildingLayers layers);
        
        IEnumerable<BoundsInt> GetDamageAreasForLayer(BuildingLayers layers);
    }
    /// <summary>
    /// strategy for choosing which tiles to damage on a building
    /// </summary>
    public interface IDamageStrategy
    {
        
    }
}