using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Cysharp.Threading.Tasks;
using Power.Steam.Network;
using UnityEngine;
using UniRx;
using Zenject;

namespace Power.Steam
{
    public class SteamNetworkState : IInitializable, IDisposable
    {
        private readonly SteamConsumers _consumers;
        private readonly SteamProducers _producers;
        private readonly NodeRegistry _steamNodes;
        private readonly CompositeDisposable _cd = new ();

        private bool logging;
        public SteamNetworkState(
            SteamConsumers consumers,
            SteamProducers producers,
            NodeRegistry steamNodes)
        {
            _consumers = consumers;
            _producers = producers;
            _steamNodes = steamNodes;
        }

        public void Initialize()
        {
            _consumers.OnSystemAdded.Subscribe(t => AddConsumer((Vector3Int)t.Item1, t.Item2));
            _producers.OnSystemAdded.Subscribe(t => AddProducer((Vector3Int)t.Item1, t.Item2));
            _consumers.OnSystemRemoved.Subscribe(t => RemoveConsumer((Vector3Int)t.Item1));
            _producers.OnSystemRemoved.Subscribe(t => RemoveProducer((Vector3Int)t.Item1));
            _steamNodes.OnValueAdded.Subscribe(t => AddPipe(t.Position, t));
            _steamNodes.OnValueRemoved.Subscribe(t => RemovePipe(t.Position));
        }

        public void Dispose()
        {
            _cd.Dispose();
        }
        
        protected virtual void AddProducer(Vector3Int position, ISteamProducer producer)
        {
            if(logging) Debug.Log($"Added producer at {position}");
        }
        protected virtual void AddConsumer(Vector3Int position, ISteamConsumer consumer)
        {
            if(logging) Debug.Log($"Added consumer at {position}");
        }
        protected virtual void AddPipe(Vector3Int position, NodeHandle node)
        {
            if(logging) Debug.Log($"Added pipe at {position}");
        }
        protected virtual void RemoveProducer(Vector3Int position)
        {
            if(logging) Debug.Log($"Removed producer at {position}");
        }
        protected virtual void RemoveConsumer(Vector3Int position)
        {
            if(logging) Debug.Log($"Removed consumer at {position}");
        }
        protected virtual void RemovePipe(Vector3Int position)
        {
            if(logging) Debug.Log($"Added pipe at {position}");
        }
    }
    
    
    
    [RequireComponent(typeof(Building))]
    public class SteamNetworkVisualizer  : MonoBehaviour
    {
        public Sprite sprite;
        [HideInInspector]  public Color consumerColor = Color.red;
        [HideInInspector]  public Color producerColor = Color.green;
        [HideInInspector]  public Gradient colorGradient = new Gradient() {
            alphaKeys = new []{ new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 1)},
            colorKeys = new []{new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1)}
        };

        public ColorConfig producerColorConfig;
        public ColorConfig consumerColorConfig;
        public ColorConfig pipeColorConfig;
    
        [Serializable]
        public class ColorConfig
        {
            public Gradient colorGradient = new Gradient() {
                alphaKeys = new []{ new GradientAlphaKey(0, 0), new GradientAlphaKey(1, 1)},
                colorKeys = new []{new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1)}
            };

            public Color GetColor(float t)
            {
                return colorGradient.Evaluate(t);
            }

            public Color GetColor(float t, float ta)
            {
                var c1 = colorGradient.Evaluate(t);
                var c2 = colorGradient.Evaluate(ta);
                c1.a = c2.a;
                return c1;
            }
        }

        private List<SpriteRenderer> _inactiveSprites = new List<SpriteRenderer>();
        private Dictionary<Vector3Int, SpriteRenderer> _spriteRenderers = new Dictionary<Vector3Int, SpriteRenderer>();
        
        private SteamNetworkController _controller;
        private Building _building;
        private Coroutine _updateVisualsCoroutine;
        private Transform _parent;
        
        
        private SteamConsumers _consumers;
        private SteamProducers _producers;
        private NodeRegistry _steamNodes;

        MyProducerDictionary _steamProducers = new ();
        MyConsumerDictionary _steamConsumers = new();
        MyPipeDictionary _pipeNodes = new();
        
        
        public bool logging = true;
        
        int ProducerCount => _steamProducers.Count;
        int ConsumerCount => _steamConsumers.Count;
        int PipeCount => _pipeNodes.Count;
        
        private class MyDictionary<T>
        {
            private Dictionary<Vector3Int, (SpriteRenderer, T)> _dictionary = new();

            public SteamNetworkVisualizer visualizer { get; set; }
            public int Count => _dictionary.Count;
            public void Add(Vector3Int position, SpriteRenderer sr, T value)
            {
                if (_updating)
                {
                    _addQueue.Enqueue((position, sr, value));
                }
                else
                {
                    _dictionary.Add(position, (sr, value));
                }
            }
            public void Remove(Vector3Int position)
            {
                if (_updating)
                {
                    _removeQueue.Enqueue(position);
                }
                else
                {
                    var sr = GetSpriteRenderer(position);
                    visualizer.RemoveSpriteAtPosition(position);
                    _dictionary.Remove(position);
                }
            }
            
            public SpriteRenderer GetSpriteRenderer(Vector3Int position) => _dictionary[position].Item1;
            public T GetValue(Vector3Int position) => _dictionary[position].Item2;
            public IEnumerable<Vector3Int> GetPositions() => _dictionary.Keys;

            public virtual void Update(Vector3Int position, SpriteRenderer sr, T value)
            {
                
            }

            bool _updating;
            Queue<Vector3Int> _removeQueue = new Queue<Vector3Int>();
            Queue<(Vector3Int, SpriteRenderer, T)> _addQueue = new ();
            public IEnumerator SlowUpdate(int countPerFrame)
            {
                int cnt = 0;
                if(_dictionary.Count == 0)yield break;
                _updating = true;
                foreach (var position in GetPositions())
                {
                    cnt++;
                    if (cnt >= countPerFrame)
                    {
                        cnt = 0;
                        yield return null;
                    }
                    Update(position, GetSpriteRenderer(position), GetValue(position));
                }
                _updating = false;
                
                while (_removeQueue.Count > 0) 
                    Remove(_removeQueue.Dequeue());
                
                while (_addQueue.Count > 0)
                {
                    var (position, sr, value) = _addQueue.Dequeue();
                    Add(position, sr, value);
                }
            }
        }

        private class MyConsumerDictionary : MyDictionary<ISteamConsumer>
        {
            public override void Update(Vector3Int position, SpriteRenderer sr, ISteamConsumer value)
            {
                var colorConfig = base.visualizer.consumerColorConfig;
                sr.color = colorConfig.GetColor(value.GetSteamConsumptionRate());
            }
        }

        private class MyProducerDictionary : MyDictionary<ISteamProducer>
        {
            public override void Update(Vector3Int position, SpriteRenderer sr, ISteamProducer value)
            {
                var colorConfig = base.visualizer.producerColorConfig;
                sr.color = colorConfig.GetColor(value.GetSteamProductionRate());
            }
        }

        private class MyPipeDictionary : MyDictionary<NodeHandle>
        {
            public override void Update(Vector3Int position, SpriteRenderer sr, NodeHandle value)
            {
                var colorConfig = base.visualizer.pipeColorConfig;
                var color = colorConfig.GetColor(value.Temperature, value.Pressure);
                sr.color = color;
            }
        }
        
        [Inject]
        public void Inject(
            SteamConsumers consumers,
            SteamProducers producers, 
            NodeRegistry steamNodes)
        {
            _consumers =consumers;
            _producers =producers;
            _steamNodes =steamNodes;

            _steamProducers.visualizer = this;
            _steamConsumers.visualizer = this;
            _pipeNodes.visualizer = this;
            _producers.OnSystemAdded.Select(t => ((Vector3Int)t.Item1, GetSpriteForPosition((Vector3Int)t.Item1, NodeType.OUTPUT), t.Item2)).Subscribe(t => _steamProducers.Add(t.Item1, t.Item2, t.Item3)).AddTo(this);
            _consumers.OnSystemAdded.Select(t => ((Vector3Int)t.Item1, GetSpriteForPosition((Vector3Int)t.Item1, NodeType.INPUT), t.Item2)).Subscribe(t => _steamConsumers.Add(t.Item1, t.Item2, t.Item3)).AddTo(this);
            _steamNodes.OnValueAdded.Select(t => (t, GetSpriteForPosition(t.Position, NodeType.PIPE))).Subscribe(t => _pipeNodes.Add(t.t.Position, t.Item2, t.t)).AddTo(this);
            
            _steamNodes.OnValueRemoved.Select(t => t.Position).Subscribe(_pipeNodes.Remove).AddTo(this);
            _producers.OnSystemRemoved.Select(t => (Vector3Int)t.Item1).Subscribe(_steamProducers.Remove).AddTo(this);
            _consumers.OnSystemRemoved.Select(t => (Vector3Int)t.Item1).Subscribe(_steamConsumers.Remove).AddTo(this);
            
            // _steamNodes.OnValueAdded.Subscribe(t => AddPipe(t.Position, t)).AddTo(this);
            // _steamNodes.OnValueRemoved.Select(t => t.Position).Subscribe(RemovePipe).AddTo(this);
            // _producers.OnSystemAdded.Subscribe(t => AddProducer((Vector3Int)t.Item1, t.Item2)).AddTo(this);
            // _producers.OnSystemRemoved.Select(t => (Vector3Int)t.Item1).Subscribe(RemoveProducer).AddTo(this);
            // _consumers.OnSystemRemoved.Select(t => (Vector3Int)t.Item1).Subscribe(RemoveConsumer).AddTo(this);
            // _consumers.OnSystemAdded.Subscribe(t => AddConsumer((Vector3Int)t.Item1, t.Item2)).AddTo(this);
        }

        


        
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

        
        IEnumerable<Vector3Int> ProducerPositions()
        {
            foreach (var producer in _consumers.GetValues())
            {
                yield return (Vector3Int)producer.cell;
            }
        }
        IEnumerable<Vector3Int> ConsumerPositions()
        {
            foreach (var consumer in _consumers.GetValues())
            {
                yield return (Vector3Int)consumer.cell;
            }
        }
        IEnumerable<Vector3Int> GetPipePositions()
        {
            foreach (var node in _steamNodes.Values)
            {
                yield return node.Position;
            }
        }

        IEnumerable<(Vector3Int, NodeType)> GetAllUsedPositions()
        {
            foreach (var pipePosition in GetPipePositions())
            {
                yield return (pipePosition, NodeType.PIPE);
            }

            foreach (var consumerPosition in ConsumerPositions())
            {
                yield return (consumerPosition, NodeType.INPUT);
            }

            foreach (var producerPosition in ProducerPositions())
            {
                yield return (producerPosition, NodeType.OUTPUT);
            }
        }
        IEnumerator UpdateVisualsAsync()
        {
            const int COUNT_PER_FRAME = 75;
            int cnt = 0;
            yield return _steamConsumers.SlowUpdate(COUNT_PER_FRAME);
            yield return _steamProducers.SlowUpdate(COUNT_PER_FRAME);
            yield return _pipeNodes.SlowUpdate(COUNT_PER_FRAME);
        }

        private SpriteRenderer GetSpriteForPosition(Vector3Int position, NodeType type)
        {
            switch (type)
            {
                case NodeType.PIPE:
                    break;
                case NodeType.INPUT:
                case NodeType.OUTPUT:
                    position.z -= 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            var sr = GetSpriteForPosition(position);
            sr.color = GetColorForNodeType(type);
            return sr;
        }

        private Color GetColorForNodeType(NodeType type)
        {
            switch (type)
            {
                case NodeType.PIPE:
                    return Color.white;
                case NodeType.INPUT:
                    return Color.green;
                case NodeType.OUTPUT:
                    return Color.red;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
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