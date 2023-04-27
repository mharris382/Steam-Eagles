using System.Collections;
using System.Collections.Generic;
using Buildings;
using UnityEngine;
using UniRx;
namespace Power.Steam
{
    [RequireComponent(typeof(Building))]
    public class SteamNetworkVisualizer  : MonoBehaviour
    {
        public Sprite sprite;
        public Color consumerColor = Color.red;
        public Color producerColor = Color.green;
        public Gradient colorGradient = new Gradient()
        {
            alphaKeys = new []{ new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 1)},
            colorKeys = new []{new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1)}
        };


        private List<SpriteRenderer> _inactiveSprites = new List<SpriteRenderer>();
        private Dictionary<Vector3Int, SpriteRenderer> _spriteRenderers = new Dictionary<Vector3Int, SpriteRenderer>();
        
        private SteamNetworkController _controller;
        private Building _building;
        private Coroutine _updateVisualsCoroutine;
        private Transform _parent;
        private void Start()
        {
            _building = GetComponent<Building>();
            _controller = SteamNetworkController.CreateControllerForBuilding(_building);
            _controller.OnNetworkUpdated.Subscribe(_ => UpdateVisuals()).AddTo(this);
            _parent = new GameObject("Pipes").transform;
            _parent.SetParent(transform);
            _parent.localPosition = Vector3.zero;
        }

        private void UpdateVisuals()
        {
            if(_updateVisualsCoroutine != null)
                StopCoroutine(_updateVisualsCoroutine);
            _updateVisualsCoroutine = StartCoroutine(UpdateVisualsAsync());
        }


        IEnumerator UpdateVisualsAsync()
        {
            const int COUNT_PER_FRAME = 25;
            int cnt = 0;
            HashSet<Vector3Int> _found = new HashSet<Vector3Int>();
            foreach (var steamNode in _controller.Network.Network.Vertices)
            {
                if (cnt > COUNT_PER_FRAME)
                {
                    cnt = 0;
                    yield return null;
                }
                cnt++;
                var cell = steamNode.Cell;
                _found.Add(cell);
                var t = steamNode.Pressure / steamNode.Capacity;
                var color = colorGradient.Evaluate(t);
                var a = color.a;
                if (steamNode is INetworkConsumer)
                {
                    color = consumerColor;
                    color.a = a;
                }
                else if (steamNode is INetworkSupplier)
                {
                    color = producerColor;
                    color.a = a;
                }

                var sr = GetSpriteForPosition(cell);
                sr.transform.localPosition = _building.Map.CellToLocal(cell, BuildingLayers.PIPE);
                sr.color = color;

            }

            Queue<Vector3Int> positionsToRemove = new Queue<Vector3Int>();
            foreach (var kvp in _spriteRenderers)
            {
                if (_found.Contains(kvp.Key) == false) positionsToRemove.Enqueue(kvp.Key);
            }
            while (positionsToRemove.Count > 0)
            {
                RemoveSpriteAtPosition(positionsToRemove.Dequeue());
            }
        }

        public SpriteRenderer GetSpriteForPosition(Vector3Int position)
        {
            if (_spriteRenderers.ContainsKey(position))
            {
                return _spriteRenderers[position];
            }

            SpriteRenderer sr;
            if(_inactiveSprites.Count > 0)
            {
                sr = _inactiveSprites[0];
                _inactiveSprites.RemoveAt(0);
                sr.gameObject.SetActive(true);
            }
            else
            {
                sr = CreateRenderer();
            }
            sr.transform.localPosition = _building.Map.CellToLocal(position, BuildingLayers.PIPE);
            _spriteRenderers.Add(position, sr);
            return sr;
        }

        public void RemoveSpriteAtPosition(Vector3Int position)
        {
            if (!_spriteRenderers.ContainsKey(position)) return;
            var sr = _spriteRenderers[position];
            sr.gameObject.SetActive(false);
            _inactiveSprites.Add(sr);
            _spriteRenderers.Remove(position);
        }

        
        private SpriteRenderer CreateRenderer()
        {
            var go = new GameObject("SteamNetworkVisualizer");
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingLayerName = "UI";
            renderer.sortingOrder = 1;
            renderer.transform.SetParent(_parent);
            return renderer;
        }
    }
}