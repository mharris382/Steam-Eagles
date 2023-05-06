using UnityEngine;
// ReSharper disable InconsistentNaming

namespace CoreLib
{
    public interface IInteractable
    {
        Transform transform { get; }
        string name { get; }
        GameObject PCVirtualCamera { get; }
    }
}