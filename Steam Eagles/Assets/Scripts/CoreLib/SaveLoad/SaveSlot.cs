using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

[Obsolete("Use Persistence Manager instead")]
[CreateAssetMenu(fileName = "New Save Slot", menuName = "Steam Eagles/Save Slot")]
public class SaveSlot : ScriptableObject
{
    [SerializeField] string savePath;

    public string SavePath
    {
        get =>  String.IsNullOrEmpty(savePath) ? name : savePath;
        set => savePath = value;
    }
    public void VerifyPathExists()
    {
        var path = $"{Application.persistentDataPath}/{SavePath}";
        if (Directory.Exists(path))
            return;
        Directory.CreateDirectory(path);
    }
    public string GetSaveDirectoryPath()
    {
        return $"{Application.persistentDataPath}/{SavePath}";
    }
    
    public SharedSaveData LoadSharedSaveData()
    {
        var path = $"{Application.persistentDataPath}/{SavePath}/SharedSaveData.json";
        if (!File.Exists(path))
            return new SharedSaveData() {
                gameStarted = false
            };
        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<SharedSaveData>(json);
    }
}