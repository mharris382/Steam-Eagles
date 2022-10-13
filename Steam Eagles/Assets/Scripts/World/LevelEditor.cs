using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace World
{
    public class LevelEditor : MonoBehaviour
    {
        private const int MAX_BRUSH_SIZE = 100;
        private const int MIN_BRUSH_SIZE = 1;
        
        [SerializeField] private Tilemap currentMap;
        [SerializeField] private Camera cam;

        [SerializeField] private BrushConfig placementBrush;
        [SerializeField] private BrushConfig eraserBrush;

        [SerializeField] private float sizeAdjustmentSpeed = 0.125f;
        public BoundsInt levelBounds = new BoundsInt(Vector3Int.zero, Vector3Int.one * 100);
        [Serializable]
        public class BrushConfig
        {
            [SerializeField]
            private Vector2Int size = Vector2Int.one;

            [SerializeField] private string brushName = "Block Brush";
            public UnityEvent<int> onSizeChanged;
            public UnityEvent<string> onSizeChangedAsLabel;
            public Vector3Int BrushSize
            {
                get => new Vector3Int(size.x <= 0 ? 1 : size.x, size.y <= 0 ? 1 : size.y);
                
            }

            public int BrushRadius
            {
                get => size.x;
                set
                {
                    size.y = size.x = Mathf.Clamp(value, MIN_BRUSH_SIZE, MAX_BRUSH_SIZE);
                    onSizeChanged?.Invoke(size.x);
                    onSizeChangedAsLabel?.Invoke($"{brushName} \n{brushShape}\t<i>size={value}</i>");
                }
            }
            [SerializeField]Shape _brushShape = Shape.Square;

            public Shape brushShape
            {
                get => _brushShape;
                set
                {
                    _brushShape = value;
                    BrushRadius = BrushRadius;
                }
            }
            HashSet<Vector3Int> found = new HashSet<Vector3Int>();
            public IEnumerable<Vector3Int> GetTiles(BoundsInt levelBounds, Vector3Int cellPosition)
            {
                var minPos = cellPosition - (BrushSize / 2);
                var maxPos = cellPosition + (BrushSize / 2);
                switch (brushShape)
                {
                    case Shape.Round:
                        int radius = BrushSize.x;
                        Vector3Int center = cellPosition;
                        for (int i = 0; i < radius; i++)
                        {
                            found.Clear();
                            int number = 1;
                            switch (radius)
                            {
                                case 1:
                                    yield return cellPosition;
                                    break;
                                case 2:
                                    number = 4;
                                    break;
                                case 3:
                                    number = 8;
                                    break;
                                default:
                                    number = 360 / 8;
                                    break;
                            }
                            for (int j = 0; j < number; j++)
                            {
                                var angleDeg = j * (360f / (float)number);
                                var x =Mathf.CeilToInt( (radius * i) * Mathf.Cos(Mathf.Deg2Rad * angleDeg));
                                var y = Mathf.CeilToInt((radius * i) * Mathf.Sin(Mathf.Deg2Rad * angleDeg));
                                yield return  center + (new Vector3Int(x, y));
                            }
                        }
                        // Vector3Int center = cellPosition + ((maxPos - minPos) / 2);
                        // for (int i = minPos.x; i < maxPos.x; i++)
                        // {
                        //     for (int j = minPos.y; j < maxPos.y; j++)
                        //     {
                        //         var dist = Vector3Int.Distance(center, new Vector3Int(i, j));
                        //         if(dist <= radius)
                        //             yield return new Vector3Int(i, j);
                        //     }
                        // }
                        break;
                    case Shape.Square:
                        for (int i = minPos.x; i < maxPos.x; i++)
                        {
                            for (int j = minPos.y; j < maxPos.y; j++)
                            {
                                yield return new Vector3Int(i, j);
                            }
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }


        }
        public enum Shape
        {
            Round,
            Square
        }
        private TileBase currentTile
        {
            get
            {
                if (!LevelManager.Instance.HasBlocks)
                    return null;
                return Blocks[_selectedTileIndex].blockStaticTile;
            }
        }

        private BlockData[] Blocks => LevelManager.Instance.Blocks;


        private int _selectedTileIndex;
        private IEnumerator Start()
        {
            while (!LevelManager.Instance.IsLoaded)
            {
                yield return null;
            }

            placementBrush.BrushRadius = placementBrush.BrushSize.x;
            eraserBrush.BrushRadius = placementBrush.BrushSize.x;
        }

        private void Update()
        {
            if (!LevelManager.Instance.HasTilemaps) return;
            if (!LevelManager.Instance.HasBlocks) return;
            if (currentTile == null) return;
            if (currentMap == null) return;
            cam = cam == null ? Camera.current : cam;

            
            var pos = currentMap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));
            var key = KeyCode.Alpha1;

            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                AdjustBrushSize(eraserBrush);
            }
            else
            {
                AdjustBrushSize(placementBrush);
            }
            
            ChangeShapeInput(KeyCode.Alpha1, placementBrush);
            ChangeShapeInput(KeyCode.Alpha2, eraserBrush);
            
            if (Input.GetMouseButton(0)) PlaceTile(pos);
            else if (Input.GetMouseButton(1)) DeleteTile(pos);

            if (Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                _selectedTileIndex++;
                _selectedTileIndex %= Blocks.Length;
            }

            if (Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                _selectedTileIndex--;
                if (_selectedTileIndex < 0) _selectedTileIndex = Blocks.Length-1;
            }
        }

        private float lastAdjustmentTime;
        private void AdjustBrushSize(BrushConfig brushConfig, KeyCode? modifierKey = null)
        {
            if(Time.realtimeSinceStartup - lastAdjustmentTime < sizeAdjustmentSpeed)
                return;
            
                var inputAxis = Input.mouseScrollDelta.y;
                if (inputAxis == 0) return;
                var adjustment = inputAxis > 0 ? 1 : -1;
                var brushSize = brushConfig.BrushSize.x;
                brushSize += adjustment;
                brushSize = Mathf.Clamp(brushSize, MIN_BRUSH_SIZE, MAX_BRUSH_SIZE);
                brushConfig.BrushRadius = brushSize;
                lastAdjustmentTime = Time.realtimeSinceStartup;
        }
        private static void ChangeShapeInput(KeyCode key, BrushConfig brush)
        {
            if (Input.GetKeyDown(key))
            {
                brush.brushShape = brush.brushShape == Shape.Round ? Shape.Square : Shape.Round;
            }
        }

        private void DeleteTile(Vector3Int pos)
        {
            if (currentMap == null)
            {
                return;
            }
            PaintWithBrush(pos, eraserBrush, null);
        }

        private void PlaceTile(Vector3Int pos)
        {
            if (currentMap == null)
            {
                return;
            }
            PaintWithBrush(pos, placementBrush, currentTile);
        }

        private void PaintWithBrush(Vector3Int pos, BrushConfig brush, TileBase tile)
        {
            if (brush.BrushSize.x > 1 || brush.BrushSize.y > 1)
            {
                var cellPositions = brush.GetTiles(levelBounds, pos).ToArray();
                var cellTiles = new TileBase[cellPositions.Length].Select(t => tile).ToArray();
                currentMap.SetTiles(cellPositions, cellTiles);
            }
            else
            {
                currentMap.SetTile(pos, tile);
            }
        }
    }
}