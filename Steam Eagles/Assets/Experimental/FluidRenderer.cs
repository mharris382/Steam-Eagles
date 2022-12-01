using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FluidRenderer : MonoBehaviour, IFluidRenderer
{
    private Renderer _renderer;
    public Renderer Renderer => _renderer ? _renderer : _renderer = GetComponent<Renderer>();
    public void SetFluidTexture(RenderTexture renderTexture)
    {
        Renderer.material.SetTexture("_MainTex", renderTexture);
    }
}