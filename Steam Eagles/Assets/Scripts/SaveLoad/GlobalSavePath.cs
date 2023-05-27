using System.IO;
using CoreLib;
using UnityEngine;

/// <summary>
/// handles logic for creating, validating, and recording the save directory path
/// </summary>
public class GlobalSavePath
{
    private readonly PersistenceConfig _config;
    public const string PLAYER_PREFS_KEY = "Last Save Path";
    private string _lastSavePath;
    private string _lastValidSavePath;

    public string FullSaveDirectoryPath
    {
        get => _lastSavePath;
        private set
        {
            _lastSavePath = GetValidDirectoryPath(value);
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, _lastSavePath);
            _config.Log($"Save Path: {_lastSavePath.Bolded()}");
        }
    }
    public bool HasSavePath => !string.IsNullOrEmpty(_lastSavePath) 
                               && Directory.Exists(_lastSavePath);
    public GlobalSavePath(PersistenceConfig config)
    {
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
}