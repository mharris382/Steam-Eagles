using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Renderer))]
    public class SharedRenderTextureVisualizer : MonoBehaviour
    {
        public Material visualizerMaterial;
        public SharedRenderTexture visualizer;

        
        private Material _localMaterial;
        private Renderer _targetRenderer;

        private Renderer Renderer => _targetRenderer != null ? _targetRenderer : (_targetRenderer = GetComponent<Renderer>());
        private Material LocalMaterial => _localMaterial != null ? _localMaterial : (_localMaterial = new Material(visualizerMaterial));

        private void Awake()
        {
            _targetRenderer = GetComponent<Renderer>();
            _localMaterial = new Material(visualizerMaterial);
            visualizer.onValueChanged.AddListener(UpdateMaterial);
        }

        private void OnDestroy()
        {
            visualizer.onValueChanged.RemoveListener(UpdateMaterial);
        }

        private void OnEnable()
        {
            
            if (visualizer.Value == null)
            {
                Renderer.enabled = false;
            }
            else
            {
                Renderer.enabled = true;
                UpdateMaterial(visualizer.Value);
            }
            Renderer.material = _localMaterial;
        }

        private void OnDisable()
        {
            Renderer.enabled = false;
        }

        void UpdateMaterial(RenderTexture renderTexture)
        {
            if (renderTexture == null)
            {
                Renderer.enabled = enabled;
            }
            else
            {
                Renderer.enabled = enabled;
                LocalMaterial.SetTexture("_RenderTexture", renderTexture);
                Renderer.material = LocalMaterial;
            }
         
        }
    }
}