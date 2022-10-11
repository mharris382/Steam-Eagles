using UnityEngine;
using World;

public class VelocityFieldRenderTexture : MonoBehaviour
{
    public SharedRenderTexture solidRenderTexture;
    public SharedRenderTexture velocityFieldTexture;

    private void Awake()
    {
        solidRenderTexture.onValueChanged.AddListener(OnSolidRenderTextureChanged);
        if (solidRenderTexture.Value != null) 
            OnSolidRenderTextureChanged(solidRenderTexture.Value);
    }

    private void OnDestroy()
    {
        solidRenderTexture.onValueChanged.RemoveListener(OnSolidRenderTextureChanged);
    }

    void OnSolidRenderTextureChanged(RenderTexture solidRenderTexture)
    {
        if (solidRenderTexture == null)
        {
               
        }
        else
        {
            int w = solidRenderTexture.width;
            int h = solidRenderTexture.height;
            var fieldTexture = new RenderTexture(w, h, 24);
            fieldTexture.enableRandomWrite = true;
            fieldTexture.Create();
            velocityFieldTexture.Value = fieldTexture;
        }
    }
}