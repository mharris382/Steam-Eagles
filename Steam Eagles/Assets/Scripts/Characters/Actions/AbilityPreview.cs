using UnityEngine;

public class AbilityPreview : MonoBehaviour
{
    public GameObject previewGameObject;
    public SpriteRenderer previewObject;

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
        if (previewObject == null) return;
        if (!enabled)
        {
            HideAbilityPreview();
            return;
        }
        if(previewGameObject!=null) previewGameObject.SetActive(true);
        previewObject.enabled = true;
        previewObject.transform.localPosition = Vector3.zero;
        previewGameObject.transform.position = wp;
    }
    internal void HideAbilityPreview()
    {
        if(previewGameObject!=null)previewGameObject.SetActive(false);
        if (previewObject == null) return;
        previewObject.enabled = false;
    }
}