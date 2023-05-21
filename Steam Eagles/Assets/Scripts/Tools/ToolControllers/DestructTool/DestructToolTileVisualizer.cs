using System;
using CoreLib.Interfaces;
using Tools.BuildTool;
using UnityEngine;
using UniRx;
namespace Tools.DestructTool
{
    [Serializable]
    public class DestructToolTileVisualizer : IDisposable
    {
        public Sprite icon;
        public Color color = new Color(0.8f, 0.2f, 0.2f, 0.7f);
        public string sortingLayerName = "UI";
        public int sortingOrder = 100;
        private SpriteRenderer _sr;
        private CompositeDisposable _cd;
        private Color _noBlockColor;
        public void OnEnable(DestructToolController destructToolController)
        {
            if (_sr == null)
            {
                Init(destructToolController);
            }
            _sr.gameObject.SetActive(true);
            _sr.size = Vector2.one*2f;
        }

        private void Init(DestructToolController destructToolController)
        {
            _cd ??= new CompositeDisposable();
            if (_sr == null)
            {
                _sr = new GameObject("Tile Visual", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                _sr.transform.SetParent(destructToolController.transform);
                _sr.sortingOrder = sortingOrder;
                _sr.sortingLayerName = sortingLayerName;
                _noBlockColor = color;
                _noBlockColor.a = 0.2f;
                _sr.sprite = icon;
                _sr.color = color;
            }
            destructToolController.OnDestructToolAimInfo
                .Select(t => t.aimInfo.Building.Map.CellToWorld(t.aimInfo.Cell))
                .Subscribe(pos => _sr.transform.position = pos).AddTo(_cd);
                
            destructToolController.OnDestructToolAimInfo.Subscribe(t => UpdateAim(t.target, t.aimInfo)).AddTo(_cd);
            this.AddTo(destructToolController);
        }

        void UpdateAim(IDestruct destruct, BuildingToolAimInfo aimInfo)
        {
            //var tile = aimInfo.Building.Map.GetTile(aimInfo.Cell);
            bool valid = destruct != null;
            _sr.color = valid ? color : _noBlockColor;
        }

        public void OnDisable()
        {
            if (_sr != null)
            {
                _sr.gameObject.SetActive(false);
            }
        }

        public void Dispose() => _cd.Dispose();
    }
}