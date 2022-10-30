using UnityEngine;
using UnityEngine.Events;

public class AbilityPreview : MonoBehaviour
{
    public GameObject previewGameObject;
    public SpriteRenderer previewObject;
    public UnityEvent onHidePreview;
    public UnityEvent onShowPreview;
    private void Awake()
    {
        previewObject = previewGameObject.GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDisable()
    {
        HideAbilityPreview();
    }

    internal void ShowAbilityPreview(Vector2 wp)
    {
        if (!enabled)
        {
            HideAbilityPreview();
            return;
        }
        
        if(previewGameObject!=null) 
            previewGameObject.SetActive(true);
        
        previewGameObject.transform.position = wp;
        onShowPreview?.Invoke();
    }
    internal void HideAbilityPreview()
    {
        if(previewGameObject!=null)
            previewGameObject.SetActive(false);
        onHidePreview?.Invoke();
    }
}