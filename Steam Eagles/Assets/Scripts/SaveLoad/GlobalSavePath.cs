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
    private readonly PathValidator _validator;
    private readonly PersistenceConfig _config;
    public const string PLAYER_PREFS_KEY = "Last Save Path";
    private string _lastSavePath;
    private string _lastValidSavePath;

    public string FullSaveDirectoryPath
    {
        get => _lastSavePath;
        private set
        {
            value = value.Replace('/', '\\');
            _lastSavePath = GetValidDirectoryPath(value);
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, _lastSavePath);
            _config.Log($"Save Path: {_lastSavePath.Bolded()}");
        }
    }

    public bool CanLoadGameFromCurrentPath
    {
        get
        {
            if (!Directory.Exists(FullSaveDirectoryPath))
            {
                return false;
            }

            return _validator.ValidatePathForLoad(FullSaveDirectoryPath);
        }
    }
    
    public bool HasSavePath => !string.IsNullOrEmpty(_lastSavePath) 
                               && Directory.Exists(_lastSavePath);
    public GlobalSavePath(PathValidator validator, PersistenceConfig config)
    {
        _validator = validator;
        _config = config;
        if (PlayerPrefs.HasKey(PLAYER_PREFS_KEY))
        {
            _lastSavePath = PlayerPrefs.GetString(PLAYER_PREFS_KEY);
        }
    }
    
    string GetValidDirectoryPath(string path)
    {
        if (!path.Contains(Application.persistentDataPath))
        {
            path = Path.Combine(Application.persistentDataPath, path);
        }
        if (Directory.Exists(path) == false)
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    
    public bool TrySetSavePath(ref string path)
    {
        if (string.IsNullOrEmpty(path))
        {
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
            if (_validator.ValidatePathForLoad(directory))
            {
                yield return directory;
            }
        }
    }

    public void Initialize()
    {
        if (!_validator.ValidatePathForLoad(_lastSavePath))
        {
            Debug.LogError("Last Save Path is invalid, reverting to default");
            foreach (var path in GetAllValidLoadPaths())
            {
                _lastValidSavePath = _lastSavePath = path;
                Debug.Log($"Found valid save path:\n {path.InItalics()}");
                return;
            }
        }
    }
}