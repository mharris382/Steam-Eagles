using System.Collections.Generic;
using System.Linq;
using Buildings;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace Buildables
{
    public class CellDebugger : MonoBehaviour
    {
        [SerializeField,Required] private SpriteRenderer cellPrefab;
        [SerializeField,Required] private Transform parent;


        private Dictionary<Vector2Int, SpriteRenderer> _usedRenderers = new();
        private Building _building;
        private Subject<IEnumerable<Vector2Int>> _debugCells = new();
        private ReadOnlyReactiveProperty<List<Vector2Int>> _lastCells;

        private void Awake()
        {
            var cells = _debugCells.Select(t => t.ToList());
            this._lastCells = cells.ToReadOnlyReactiveProperty();
            _lastCells.SelectMany(t => t).Subscribe(Show).AddTo(this);
        }

        [Inject] void InjectMe(Building building)
        {
            _building = building;
        }

        private bool HasResources() => _building != null;

        public void Debug(IEnumerable<Vector2Int> cells)
        {
            if (_lastCells.HasValue) _lastCells.Value.ForEach(Hide);
            _debugCells.OnNext(cells);
        }
        private void Hide(Vector2Int position)
        {
            var sr = GetRenderer(position);
            sr.enabled = false;
        }
        private void Show(Vector2Int position)
        {
            var sr = GetRenderer(position);
            sr.enabled = true;
        }
        private SpriteRenderer GetRenderer(Vector2Int position)
        {
            if (!_usedRenderers.TryGetValue(position, out var sr))
            {
                sr = Instantiate(cellPrefab, parent);
                sr.transform.position = _building.Map.CellToWorldCentered((Vector3Int)position, DebugLayer);
                _usedRenderers.Add(position, sr);
            }
            return sr;
        }

        protected virtual BuildingLayers DebugLayer => BuildingLayers.SOLID;
    }
}