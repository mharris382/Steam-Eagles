using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    public class LevelEditor : MonoBehaviour
    {
        [SerializeField] private Tilemap currentMap;
        [SerializeField] private Camera cam;
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
        }

        private void Update()
        {
            if (!LevelManager.Instance.HasTilemaps) return;
            if (!LevelManager.Instance.HasBlocks) return;
            if (currentTile == null) return;
            if (currentMap == null) return;
            cam = cam == null ? Camera.current : cam;

            var pos = currentMap.WorldToCell(cam.ScreenToWorldPoint(Input.mousePosition));

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

        private void DeleteTile(Vector3Int pos)
        {
            if (currentMap == null)
            {
                return;
            }
            currentMap.SetTile(pos, null);
        }

        private void PlaceTile(Vector3Int pos)
        {
            if (currentMap == null)
            {
                return;
            }
            currentMap.SetTile(pos, currentTile);
        }
    }
}