using System;
using System.Collections.Generic;
using System.Linq;
using Characters.Actions;
using UnityEngine;

public class Ability : MonoBehaviour
{
    public CellSelector selector;
    public CellAbility cellAbility;
    
  
    public AbilityPreview abilityPreview;
    private List<Vector3Int> selectedCells;
    public bool previewingMouse = false;
    public bool showPreview = true;
    
    public Comparison<Vector3Int> CustomSortFunction { get; set; }

    private AbilityUser _user;
    public AbilityUser User
    {
        get
        {
            if (_user == null)
            {
                _user = GetComponentInParent<AbilityUser>();
            }
            return _user;
        }
    }
    
    private void Awake()
    {
        selectedCells = new List<Vector3Int>();
    }

    private void Update()
    {
        if (selector.Ready == false)
        {
            HideAbilityPreview();
        }
        
        selectedCells = selector.GetSelectedCells().ToList();
        if (previewingMouse) return;
        if (selectedCells.Count > 0)
        {
            foreach (var selectedCell in selectedCells)
            {
                if (cellAbility.CanPerformAbilityOnCell(User,selectedCell))
                {
                    ShowAbilityPreview(cellAbility.Tilemap.GetCellCenterWorld(selectedCell));
                    return;
                }
            }
        }
        HideAbilityPreview();
    }
    
    public bool TryAbility()
    {
        if (selector.Ready == false) return false;
        if (selectedCells == null || selectedCells.Count == 0) return false;
        if (cellAbility.Tilemap == null) return false;
        foreach (var selectedCell in selectedCells)
        {
            if (cellAbility.CanPerformAbilityOnCell(User, selectedCell))
            {
                cellAbility.PerformAbilityOnCell(User, selectedCell);
                return true;
            }
        }
        return false;
    }

    private int Sort(Vector3Int c1, Vector3Int c2)
    {
        var diff = c1 - c2;
        if (diff.sqrMagnitude == 0) return 0;
        return diff.sqrMagnitude > 0 ? -1 : 1;
        if (CustomSortFunction != null)
        {
            return CustomSortFunction.Invoke(c1, c2);
        }
        return cellAbility.SortCellLocationsByPreference(c1, c2);
    }

    public bool TryAbility(Vector3 center, Vector3 direction, float dot = 0)
    {
        float Dot(Vector3Int t)
        {
            var wp = cellAbility.Tilemap.GetCellCenterWorld(t);
            return Vector2.Dot((wp - center).normalized, direction.normalized);
        }

        dot = Mathf.Clamp(dot, -1, 1);
        foreach (var selectedCell in selectedCells
                     .Where(t => cellAbility.CanPerformAbilityOnCell(User,t))
                     .OrderByDescending(Dot))
        {
            var d = Dot(selectedCell);
            if (d < dot)
                continue;
            cellAbility.PerformAbilityOnCell(User, selectedCell);
            return true;
        }
        
        return false;
    }
    public bool TryAbility(Vector3 tPos, float maxDistance = 0)
    {
        float CompareDist(Vector3Int t)
        {
            var wp = cellAbility.Tilemap.GetCellCenterWorld(t);
            return (wp - tPos).sqrMagnitude;
        }

      
        foreach (var selectedCell in selectedCells
                     .Where(t => cellAbility.CanPerformAbilityOnCell(User,t))
                     .OrderByDescending(CompareDist))
        {
            var d = CompareDist(selectedCell);
            if (maxDistance > 0 && CompareDist(selectedCell)< Mathf.Pow(maxDistance, 2))
                continue;
            cellAbility.PerformAbilityOnCell(User, selectedCell);
            return true;
        }
        
        return false;
    }

    IEnumerable<(Vector3Int cell, Vector3 wp)> GetCellsFromMousePosition(Vector3 mp)
    {
        float CompareDist(Vector3Int t)
        {
            var wp = cellAbility.Tilemap.GetCellCenterWorld(t);
            return (wp - mp).sqrMagnitude;
        }

        return selectedCells.Where(cellPosition => cellAbility.CanPerformAbilityOnCell(User,cellPosition))
            .OrderByDescending(CompareDist).Select(t => (t, cellAbility.Tilemap.GetCellCenterWorld(t)));
    }

    #region Preview Helpers
    
    private void HideAbilityPreview()
    {
        if (abilityPreview == null) return;
        abilityPreview.HideAbilityPreview();
    }

    public void ShowAbilityPreview(Vector2 wp)
    {
        if (abilityPreview == null) return;
        
        abilityPreview.ShowAbilityPreview(wp);
    }
    public void ShowAbilityPreviewFromMouse(Vector3 wp)
    {
        if (abilityPreview == null) return;
        previewingMouse = true;
        var cells = GetCellsFromMousePosition(wp).ToArray();
        if (cells == null || cells.Length == 0)
        {
            HideAbilityPreview();
        }
        else
        {
            try
            {
                var best = cells.First();
                ShowAbilityPreview(best.wp);
            }
            catch (Exception e)
            {
                Debug.LogError($"Cells failed: {e}");
            }
            
        }
    }
    
    #endregion


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (selector.Ready == false) return;
        if (selectedCells == null || selectedCells.Count == 0)
            return;
        if (cellAbility.Tilemap == null) return;
        bool isFirst = true;
        var validColor = Color.Lerp(Color.clear, Color.green, 0.8f);
        var invalidColor = Color.Lerp(Color.clear, Color.red, 0.2f);
        foreach (var cell in selectedCells.Where(t => cellAbility.CanPerformAbilityOnCell(User,t)))
        {
            var wp = cellAbility.Tilemap.GetCellCenterWorld(cell);
            
            var color = cellAbility.CanPerformAbilityOnCell(User,cell) ? validColor : invalidColor;
            Gizmos.color =Color.Lerp(Color.clear, Color.green,  isFirst ? 1 : 0.4f);
            isFirst = false;
            float r = 0.5f;
            Gizmos.DrawSphere(wp, r);
        }
    }
#endif


   
}

