using System;
using UnityEngine;

/// <summary>
/// since the ability system is getting newer more complex features added, I think it's important to setup a shared data container to hold shared state information
/// and act as a dependency injection entry point for other systems to interface with.
/// </summary>
public class AbilityUser : MonoBehaviour
{
    public bool infiniteResources;
    public int initialPipes = 10;
    public int initialBlocks = 10;
    
    int _storedPipes;
    public int StoredPipes
    {
        get {
            if(infiniteResources)return int.MaxValue;
            return _storedPipes;
        }
        set => _storedPipes =value;
    }

    int _storedBlocks;
    public int StoredBlocks
    {
        get {
            if(infiniteResources)return int.MaxValue;
            return _storedBlocks;
        }
        set => _storedBlocks =value;
    }

    private void Awake()
    {
        _storedBlocks = initialBlocks;
        _storedPipes = initialPipes;
    }
}