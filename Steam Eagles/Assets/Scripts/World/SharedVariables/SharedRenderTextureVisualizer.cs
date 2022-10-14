using UnityEngine;

namespace World
{
    public class SharedRenderTextureVisualizer : MonoBehaviour
    {
        public Material visualizerMaterial;
        public Camera camera;
        public Renderer targetRenderer;
        public SharedRenderTexture visualizer;

        private Material _localMaterial;
        
        private Material LocalMaterial => _localMaterial == null ? (_localMaterial = new Material(visualizerMaterial)) : _localMaterial;

        private void Awake()
        {
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
                targetRenderer.enabled = false;
            }
            else
            {
                targetRenderer.enabled = true;
                
            }
            targetRenderer.material = _localMaterial;
        }

        private void OnDisable()
        {
            targetRenderer.enabled = false;
        }

        void UpdateMaterial(RenderTexture renderTexture)
        {
            if (renderTexture == null)
            {
                targetRenderer.enabled = enabled;
            }
            else
            {
                targetRenderer.enabled = enabled;
                LocalMaterial.SetTexture("renderTexture", renderTexture);
            }
         
        }
    }
}