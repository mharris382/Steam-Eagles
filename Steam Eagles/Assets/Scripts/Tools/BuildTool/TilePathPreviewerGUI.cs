using System;
using System.Collections.Generic;
using Buildings;
using Buildings.Tiles;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace Tools.BuildTool
{
    public class TilePathPreviewerGUI : MonoBehaviour
    {
        [Required, AssetsOnly]
        public SpriteRenderer tilePrefab;
        [ AssetsOnly]
        public SpriteRenderer hoveredPrefab;
        public Color validColor = Color.green;
        public Color invalidColor = Color.red;
        
        private IReadOnlyReactiveProperty<IEditableTile> _selectedTile;
        private IObservable<List<Vector3Int>> _selectedTilePath;
        private BuildingMap _buildingMap;

        private bool HasBeenSetup => _selectedTile != null;

        
        private Queue<SpriteRenderer> _pooledSprites = new Queue<SpriteRenderer>();
        private List<SpriteRenderer> _activeSprites = new List<SpriteRenderer>();

        private SpriteRenderer _hoveredSprite;
        private Building _building;

        private void Awake()
        {
            CreateHoverSprite();
        }

        private void CreateHoverSprite()
        {
            if (_hoveredSprite != null)
                return;
            if (hoveredPrefab == null) hoveredPrefab = tilePrefab;
            _hoveredSprite = Instantiate(hoveredPrefab, transform);
            _hoveredSprite.gameObject.SetActive(false);
        }

        SpriteRenderer GetSprite()
        {
            if (_pooledSprites.Count > 0)
            {
                return _pooledSprites.Dequeue();
            }
            else
            {
                var sprite = Instantiate(tilePrefab, transform);
                
                return sprite;
            }
        }

        void ShowPath(List<Vector3Int> path)
        {
            if (_activeSprites.Count < path.Count)
            {
                //add new sprites
                for (int i = _activeSprites.Count; i < path.Count; i++)
                {
                    var sprite = GetSprite();
                    _activeSprites.Add(sprite);
                    sprite.gameObject.SetActive(true);
                    sprite.transform.SetParent(_building.transform);
                }
            }
            else if (_activeSprites.Count > path.Count)
            {
                //remove sprites
                for (int i = _activeSprites.Count - 1; i >= path.Count; i--)
                {
                    var sprite = _activeSprites[i];
                    _activeSprites.RemoveAt(i);
                    _pooledSprites.Enqueue(sprite);
                    sprite.gameObject.SetActive(false);
                }
            }
            
            for (int i = 0; i < path.Count; i++)
            {
                var sprite = _activeSprites[i];
                var cell = path[i];
                var worldPos = GetWorldPosition(cell);
                sprite.transform.SetParent(_building.transform);
                sprite.transform.position = worldPos;
                sprite.size = _buildingMap.GetCellSize(_selectedTile.Value.GetLayer());
            }
        }
        
        
        
        public void Setup(
            IReadOnlyReactiveProperty<Vector3Int> hoveredTile,
            IReadOnlyReactiveProperty<IEditableTile> selectedTile,
            IObservable<List<Vector3Int>> selectedTilePath,
            BuildingMap buildingMap,
            Building building)
        {
            _building = building;
          CreateHoverSprite();
            _selectedTile = selectedTile;
            _selectedTilePath = selectedTilePath;
            _buildingMap = buildingMap;
            hoveredTile.Where(_ => _selectedTile.Value != null)
                .Subscribe(cell =>
                {
                    var wsPos = buildingMap.CellToWorld(cell, selectedTile.Value.GetLayer());
                    _hoveredSprite.transform.position = wsPos;
                    bool isValid = selectedTile.Value.IsPlacementValid(cell, buildingMap);
                    _hoveredSprite.color = (isValid ? validColor : invalidColor).Lighten(0.25f);
                    _hoveredSprite.gameObject.SetActive(true);
                }).AddTo(this);
            selectedTile.Select(t => t != null).Subscribe(hasTile =>
            {
                _hoveredSprite.gameObject.SetActive(hasTile);
            });
            _selectedTilePath.Where(_ => _selectedTile.Value != null)
                .Subscribe(ShowPath)
                .AddTo(this);
        }

        Vector3 GetWorldPosition(Vector3Int cell) => _buildingMap.CellToWorld(cell, _selectedTile.Value.GetLayer());
    }
}