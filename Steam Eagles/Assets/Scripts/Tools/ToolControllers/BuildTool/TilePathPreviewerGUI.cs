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

        private void OnEnable()
        {
            if(_hoveredSprite != null)
                _hoveredSprite.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if(_hoveredSprite != null)
                _hoveredSprite.gameObject.SetActive(false);
            foreach (var sprite in _activeSprites)
            {
                sprite.gameObject.SetActive(false);
                _pooledSprites.Enqueue(sprite);
            }
            _activeSprites.Clear();
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
        public IDisposable Setup(
            IReadOnlyReactiveProperty<Vector3Int> hoveredTile,
            IReadOnlyReactiveProperty<IEditableTile> selectedTile,
            IObservable<List<Vector3Int>> selectedTilePath,
            IReadOnlyReactiveProperty<Building> building)
        {
            _building = building.Value;
            
            CreateHoverSprite();
            _selectedTile = selectedTile;
            _selectedTilePath = selectedTilePath;
            if(_building) _buildingMap = _building.Map;
            var cd = new CompositeDisposable();
            
            building.Subscribe(b => {
                if (b != null) {
                    _building = b;
                    _buildingMap = b.Map;
                }
            }).AddTo(cd);
            building.Select(t => t != null).Subscribe(SetVisible).AddTo(cd);



            var d = hoveredTile.Where(_ => _selectedTile.Value != null)
                .Subscribe(cell =>
                {
                    var wsPos = _buildingMap.CellToWorld(cell, selectedTile.Value.GetLayer());
                    _hoveredSprite.transform.position = wsPos;
                    bool isValid = selectedTile.Value.IsPlacementValid(cell, _buildingMap);
                    _hoveredSprite.color = (isValid ? validColor : invalidColor).Lighten(0.25f);
                }).AddTo(cd);
            
            
            var hasPath = _selectedTilePath.Select(t => t.Count > 1);
            hasPath.Subscribe(t => _hoveredSprite.gameObject.SetActive(t)).AddTo(cd);
            
            selectedTile.Select(t => t != null).Subscribe(hasTile => _hoveredSprite.gameObject.SetActive(hasTile)).AddTo(cd);
            _selectedTilePath.Where(_ => _selectedTile.Value != null)
                .Subscribe(ShowPath).AddTo(cd).AddTo(this);
            return cd;
        }

        public void SetVisible(bool visible)
        {
            _hoveredSprite.gameObject.SetActive(visible);
        }
        
        
        public IDisposable Setup(
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

            var cd = new CompositeDisposable();
            var d =  hoveredTile.Where(_ => _selectedTile.Value != null)
                .Subscribe(cell =>
                {
                    var wsPos = buildingMap.CellToWorld(cell, selectedTile.Value.GetLayer());
                    _hoveredSprite.transform.position = wsPos;
                    bool isValid = selectedTile.Value.IsPlacementValid(cell, buildingMap);
                    _hoveredSprite.color = (isValid ? validColor : invalidColor).Lighten(0.25f);
                });
            d.AddTo(cd);
            d.AddTo(this);
            var hasPath = _selectedTilePath.Select(t => t.Count > 1);
            hasPath.Subscribe(t => _hoveredSprite.gameObject.SetActive(t)).AddTo(cd);
            //var pathEnd = _selectedTilePath
            //    .Where(t => t is { Count: > 0 })
            //    .Select(t => t[^1]);
//
            //hasPath.Select(t => t ? pathEnd : hoveredTile).Switch().Where(t => _selectedTile.Value != null)
            //    .Subscribe(SetEndPosition).AddTo(this);
            
            selectedTile.Select(t => t != null).Subscribe(hasTile =>
            {
                _hoveredSprite.gameObject.SetActive(hasTile);
            }).AddTo(cd);
            
            d = _selectedTilePath.Where(_ => _selectedTile.Value != null)
                .Subscribe(ShowPath);
                d.AddTo(this);
                d.AddTo(cd);
                return cd;
        }

        void SetEndPosition(Vector3Int cell)
        {
            var wsPos = GetWorldPosition(cell);
            _hoveredSprite.transform.position = wsPos;
        }
        
        Vector3 GetWorldPosition(Vector3Int cell) => _buildingMap.CellToWorld(cell, _selectedTile.Value.GetLayer());

        public void MoveSpriteTo(Vector3 cellToWorld)
        {
            _hoveredSprite.transform.position = cellToWorld;
        }
    }
}