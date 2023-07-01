using UniRx;
using UnityEngine;

public class FMODSceneLoadCallback : MonoBehaviour
{
    private ReactiveProperty<bool> _isSceneLoaded = new();
    public IReadOnlyReactiveProperty<bool> IsSceneLoaded => _isSceneLoaded;
    public void OnSceneLoaded()
    {
        _isSceneLoaded.Value = true;
    }
}