using System.Collections.Generic;
using System.Linq;
using Characters.Actions;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public CellSelector selector;
    public CellAbility cellAbility;

    public GameObject previewGameObject;
    public SpriteRenderer previewObject;

    private List<Vector3Int> selectedCells;

    public bool showPreview = true;
    private void Update()
    {
        if (selector.Ready == false)
        {
            HideAbilityPreview();
        }

        selectedCells = selector.GetSelectedCells().ToList();
        selectedCells.Sort((c1, c2) => cellAbility.SortCellLocationsByPreference(c1, c2));
        if (selectedCells.Count > 0)
        {
            foreach (var selectedCell in selectedCells)
            {
                if (cellAbility.CanPerformAbilityOnCell(selectedCell))
                {
                    ShowAbilityPreview(cellAbility.Tilemap.GetCellCenterWorld(selectedCell));
                    return;
                }
            }
        }
        HideAbilityPreview();
        
    }

    private void HideAbilityPreview()
    {
        if (previewObject == null) return;
        if(previewGameObject!=null)previewGameObject.SetActive(false);
        previewObject.enabled = false;
    }

    private void ShowAbilityPreview(Vector2 wp)
    {
        if (previewObject == null) return;
        if (!showPreview)
        {
            HideAbilityPreview();
            return;
        }
        if(previewGameObject!=null)previewGameObject.SetActive(true);
        previewObject.enabled = true;
        previewObject.transform.position = wp;
    }


    private void OnDrawGizmos()
    {
        if (selector.Ready == false) return;
        if (selectedCells == null || selectedCells.Count == 0)
            return;
        if (cellAbility.Tilemap == null) return;
        bool isFirst = true;
        var validColor = Color.Lerp(Color.clear, Color.green, 0.8f);
        var invalidColor = Color.Lerp(Color.clear, Color.red, 0.2f);
        foreach (var cell in selectedCells.Where(t => cellAbility.CanPerformAbilityOnCell(t)))
        {
            var wp = cellAbility.Tilemap.GetCellCenterWorld(cell);
            
            var color = cellAbility.CanPerformAbilityOnCell(cell) ? validColor : invalidColor;
            Gizmos.color =Color.Lerp(Color.clear, Color.green,  isFirst ? 1 : 0.4f);
            isFirst = false;
            float r = 0.5f;
            Gizmos.DrawSphere(wp, r);
        }
    }

    public bool TryAbility()
    {
        if (selector.Ready == false) return false;
        if (selectedCells == null || selectedCells.Count == 0) return false;
        if (cellAbility.Tilemap == null) return false;
        foreach (var selectedCell in selectedCells)
        {
            if (cellAbility.CanPerformAbilityOnCell(selectedCell))
            {
                cellAbility.PerformAbilityOnCell(selectedCell);
                return true;
            }
        }
        return false;
    }
}