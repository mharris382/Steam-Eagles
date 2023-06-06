using System;
using System.Collections.Generic;
using QuikGraph;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utilities
{
    public abstract class GraphDrawer : MonoBehaviour , IPositionMap
    {
      
       [Required] public Grid grid;
        public GraphDrawerConfig config;
        public IPositionMap positionMap;
        public SpriteRendererFactory spriteRendererFactory;

        public Vector3 offset;
        private void Awake()
        {
            positionMap = this;
            spriteRendererFactory = new SpriteRendererFactory(config, transform, positionMap);
        }

        public Vector3 GetPosition(Vector2Int position) => grid.CellToWorld((Vector3Int)position) + offset;


        [Button(ButtonSizes.Large)]
        public void Draw()
        {
            var graph = GetGraph();
            foreach (var edge in graph.Edges)
            {
                spriteRendererFactory.GetEdgeRenderer(edge);
            }

            foreach (var vert in graph.Vertices)
            {
                spriteRendererFactory.GetNodeRenderer(vert);
            }
        }
        public abstract AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> GetGraph();
    }
    [Serializable]
    public class GraphDrawerConfig
    {
      [NaughtyAttributes.Required]  public Sprite nodeSprite;
      [NaughtyAttributes.Required]  public Sprite edgeSprite;
        
        public float nodeSize = 1f;
        public Vector2 edgeSize = new Vector2(1f, 0.1f);
    }

    

    public interface IPositionMap
    {
        Vector3 GetPosition(Vector2Int position);
    }
    
    public class SpriteRendererFactory
    {
        private readonly GraphDrawerConfig _config;

        private Dictionary<SEdge<Vector2Int>, SpriteRenderer> _edgeRenderers = new Dictionary<SEdge<Vector2Int>, SpriteRenderer>();
        private Dictionary<Vector2Int, SpriteRenderer> _nodeRenderers = new Dictionary<Vector2Int, SpriteRenderer>();
        private Transform _parent;
        private readonly IPositionMap _positionMap;

        public SpriteRendererFactory(GraphDrawerConfig config,Transform parent, IPositionMap positionMap)
        {
            _config = config;
            _parent = parent;
            _positionMap = positionMap;
        }
        
     
        public SpriteRenderer GetNodeRenderer(Vector2Int position)
        {
            if (_nodeRenderers.TryGetValue(position, out var renderer))
            {
                return renderer;
            }
            else
            {
                var go = new GameObject($"Node {position}");
                go.transform.parent = _parent;
                renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _config.nodeSprite;
                renderer.transform.localScale = new Vector3(_config.nodeSize, _config.nodeSize, 1f);
                renderer.transform.position = _positionMap.GetPosition(position);
                _nodeRenderers.Add(position, renderer);
                return renderer;
            }
        }

        public SpriteRenderer GetEdgeRenderer(SEdge<Vector2Int> edge)
        {
            if (_edgeRenderers.TryGetValue(edge, out var renderer))
            {
                return renderer;
            }
            else
            {
                var go = new GameObject($"Edge {edge.Source} -> {edge.Target}");
                go.transform.parent = _parent;
                renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = _config.edgeSprite;
                renderer.transform.localScale = new Vector3(_config.edgeSize.x, _config.edgeSize.y, 1f);
                renderer.transform.position = (_positionMap.GetPosition(edge.Source) + _positionMap.GetPosition(edge.Target))/2f;
                renderer.transform.right = _positionMap.GetPosition(edge.Target) - _positionMap.GetPosition(edge.Source);
                _edgeRenderers.Add(edge, renderer);
                return renderer;
            }
        }
        
    }
}