using UnityEngine;

namespace CoreLib
{
    public interface IIconable
    {
        
        // ReSharper disable once InconsistentNaming
        string name { get; }
        Sprite GetIcon();
    }
}