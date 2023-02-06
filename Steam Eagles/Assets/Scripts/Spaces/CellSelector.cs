using System;
using CoreLib;
using CoreLib.SharedVariables;
using StateMachine;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Spaces
{
    public class CellSelector : MonoBehaviour, ICellSelector
    {
                
        [SerializeField] private TransformAsCellSelector cellSelector;
        [SerializeField] private CursorAsCellSelector mouseSelector;

        [SerializeField] private SharedBool useCursor;
        
        private abstract class CellSelectorBase: ICellSelector
        {
            private Grid _grid;
            public Grid Grid
            {
                set => _grid = value;
                get => _grid;
            }
            public abstract Vector3Int GetSelectedCell();
            public void SetTargetTilemap(Tilemap tilemap)
            {
                this.TargetTilemap = tilemap;
            }

            private Tilemap _tilemap;

            public Tilemap TargetTilemap
            {
                get => _tilemap == null
                    ? (_tilemap = GameObject.FindGameObjectWithTag("Pipe Tilemap").GetComponent<Tilemap>())
                    : _tilemap;
                set
                {
                    if (value == null)
                    {
                        Debug.LogError("WHY DO?");
                        return;
                    }
                    _tilemap = value;
                }
            }
        }

        [Serializable]
        private class TransformAsCellSelector : CellSelectorBase
        {
            [SerializeField]
            private SharedTransform selectedCell;

            
            public override Vector3Int GetSelectedCell()
            {
                var wp = selectedCell.Value.position;
                return TargetTilemap.WorldToCell(wp);
            }
        }
        
        
        [Serializable]
        private class CursorAsCellSelector : CellSelectorBase
        {
            public SharedCamera playerCamera;
            public override Vector3Int GetSelectedCell()
            {
                Debug.Assert(playerCamera != null, "No Player Camera Assigned to Cursor as Cell Selector inside PipeBuilder");
                var wp = playerCamera.Value.ScreenToWorldPoint(Input.mousePosition);
                
                return TargetTilemap.WorldToCell(wp);
            }
        }


        private ICellSelector CurrentSelector =>
            useCursor == null || useCursor.Value == false ? cellSelector : mouseSelector;

        public Vector3Int GetSelectedCell() => CurrentSelector.GetSelectedCell();

        public void SetTargetTilemap(Tilemap tilemap)
        {
            cellSelector.SetTargetTilemap(tilemap);
            mouseSelector.SetTargetTilemap(tilemap);
        }
    }
}