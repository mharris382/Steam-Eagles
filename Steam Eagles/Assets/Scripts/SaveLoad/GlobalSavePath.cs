using System.Collections.Generic;
using System.IO;
using CoreLib;
using UnityEngine;
using Zenject;

/// <summary>
/// handles logic for creating, validating, and recording the save directory path
/// </summary>
public class GlobalSavePath : IInitializable
{
    // private readonly PathValidator _validator;
    private readonly PersistenceConfig _config;
    public const string PLAYER_PREFS_KEY = "Last Save Path";
    private string _lastSavePath;
    private string _lastValidSavePath;

    #region [Properties]

    public string FullSaveDirectoryPath
    {
        get => _lastSavePath;
        private set
        {
            _lastSavePath = AllowSavingOutsidePersistentDataPath ? value : GetValidDirectoryPath(value);
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, _lastSavePath);
            _config.Log($"Save Path: {_lastSavePath.Bolded()}");
        }
    }

    public bool AllowSavingOutsidePersistentDataPath { get; set; } = false;
  
    public bool HasSavePath => !string.IsNullOrEmpty(_lastSavePath) 
                               && Directory.Exists(_lastSavePath);

    #endregion
    public GlobalSavePath( PersistenceConfig config)
    {
        
        _config = config;
        if (PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            _lastSavePath = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
        }
    }

    public bool TrySetSavePath(ref string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Save Path is null or empty");
            return false;
        }
        FullSaveDirectoryPath = path;
        return true;
    }

    public void RevertToLastValidSavePath()
    {
        FullSaveDirectoryPath = _lastValidSavePath;
    }

    public void ConfirmPathIsValid()
    {
        _lastValidSavePath = FullSaveDirectoryPath;
    }

    public IEnumerable<string> GetAllValidLoadPaths()
    {
        var root = Application.persistentDataPath;
        var directories = Directory.GetDirectories(root);
        foreach (var directory in directories)
        {
            // if (_validator.ValidatePathForLoad(directory))
            // {
            
                yield return directory;
            // }
        }
    }

    public void Initialize()
    {
        // if (!_validator.ValidatePathForLoad(_lastSavePath))
        // {
        //     Debug.LogWarning("Last Save Path is invalid, reverting to default");
        //     foreach (var path in GetAllValidLoadPaths())
        //     {
        //         _lastValidSavePath = _lastSavePath = path;
        //         Debug.Log($"Found valid save path:\n {path.InItalics()}");
        //         return;
        //     }
        // }
    }

    private string GetValidDirectoryPath(string path)
    {
        if (!AllowSavingOutsidePersistentDataPath && !path.Contains(Application.persistentDataPath))
        {
            path = Path.Combine(Application.persistentDataPath, path);
        }
        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public bool StartsWith(string persistentDataPath)
    {
        throw new System.NotImplementedException();
    }
}