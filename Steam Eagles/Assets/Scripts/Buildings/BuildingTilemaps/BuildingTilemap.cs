using System;
using System.Collections.Generic;
using Damage;
using PhysicsFun.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings.BuildingTilemaps
{
    [RequireComponent(typeof(Tilemap))]
    public abstract class BuildingTilemap : MonoBehaviour, IBuildingTilemap
    {
        private Tilemap _tm;
        public Tilemap Tilemap => _tm ? _tm : _tm = GetComponent<Tilemap>();

        private Building _building;
        
        public abstract BuildingLayers Layer { get; }

        public abstract string GetSaveID();
        
        [Obsolete("Removed automatic update")]
        public virtual void UpdateTilemap(Building building)
        {
            name = $"{building.buildingName} {GetType().Name}";
        }

        private void Awake()
        {
            _building = GetComponentInParent<Building>();
        }
        
        
        public Dictionary<TileBase, List<Vector2Int>> TilePositionDictionary { get; }
        
        public Rect GetWorldBounds()
        {
            var bounds = Tilemap.cellBounds;
            var min = Tilemap.CellToWorld(bounds.min);
            var max = Tilemap.CellToWorld(bounds.max);
            return new Rect(min, max - min);
        }

        public string StructureName => _building.buildingName;
        public Vector2 CellSize => Tilemap.cellSize * new Vector2(transform.localScale.x, transform.localScale.y);
        public BoundsInt CellBounds => Tilemap.cellBounds;
        public TileBase GetTile(Vector2Int position) => Tilemap.GetTile(new Vector3Int(position.x, position.y, 0));
        public T GetTile<T>(Vector2Int position) where T : TileBase => Tilemap.GetTile<T>(new Vector3Int(position.x, position.y, 0));
        
        public IEnumerable<(TileBase t, Vector3Int c)> GetAllNonEmptyTiles()
        {
            for (int x = 0; x < CellBounds.x; x++)
            {
                for (int y = 0; y < CellBounds.y; y++)
                {
                    for (int z = 0; z < CellBounds.z; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        var tile = Tilemap.GetTile(cell);
                        if (tile != null)
                        {
                            yield return (tile, cell);
                        }
                    }
                }
            }
        }
        public IEnumerable<Vector3Int> GetAllEmptyTiles()
        {
            for (int x = 0; x < CellBounds.x; x++)
            {
                for (int y = 0; y < CellBounds.y; y++)
                {
                    for (int z = 0; z < CellBounds.z; z++)
                    {
                        var cell = new Vector3Int(x, y, z);
                        var tile = Tilemap.GetTile(cell);
                        if (tile == null)
                        {
                            yield return cell;
                        }
                    }
                }
            }
        }

        public void SetTile(Vector2Int position, TileBase tile) => Tilemap.SetTile(new Vector3Int(position.x, position.y, 0), tile);
    }
}