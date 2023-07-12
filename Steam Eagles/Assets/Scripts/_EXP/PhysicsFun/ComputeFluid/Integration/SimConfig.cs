using System;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class SimConfig
{
    private const int MIN = 1;
    private const int MAX = 4;

    [ValueDropdown(nameof(GetResolutions))]
    [SerializeField] int resolution = 1;

    public float updateRate = 0.25f;

    #region [Editor Helper]

    ValueDropdownList<int> GetResolutions()
    {
        var list = new ValueDropdownList<int>();
        for (int i = MIN; i <= MAX; i++)
        {
            list.Add(Mathf.Pow(i, 2).ToString(CultureInfo.InvariantCulture), i);
        }
        return list;
    }

    #endregion

    public Vector3 GetGridCellSize()
    {
        float size = 1 / (float)resolution;
        return new Vector3(size, size, 1);
    }
}