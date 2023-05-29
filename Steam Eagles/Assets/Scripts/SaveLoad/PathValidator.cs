using System.Collections.Generic;
using System.IO;
using CoreLib;
using UnityEngine;

public class PathValidator
{
    private readonly List<ISaveLoaderSystem> _saveLoadSystems;

    public PathValidator(List<ISaveLoaderSystem> saveLoadSystems)
    {
        _saveLoadSystems = saveLoadSystems;
    }

    public bool ValidatePathForLoad(string directoryPath)
    {
        bool allFilesFound = true;
        if (directoryPath == Application.persistentDataPath)
            return false;
        directoryPath = directoryPath.Replace("/", "\\");
        if (Directory.Exists(directoryPath) == false)
            return false;
        List< (string, ISaveLoaderSystem)> missingFiles = new List<(string, ISaveLoaderSystem)>();
        foreach (var system in _saveLoadSystems)
        {
            if(system.IsSystemOptional())
                continue;
            foreach (var saveFileName in system.GetSaveFileNames())
            {
                string fullPath = Path.Combine(directoryPath, $"{saveFileName.name}.{saveFileName.ext}" );
                if (File.Exists(fullPath) == false)
                {
                    Debug.LogError($"File not found: {fullPath.Bolded().InItalics().ColoredRed()}");
                    allFilesFound = false;
                    missingFiles.Add((fullPath, system));
                }
            }
        }

        if (!allFilesFound)
        {
            Debug.LogError($"Missing Files: {missingFiles.Count}");
            foreach (var missingFile in missingFiles)
            {
                Debug.LogError($"Missing File: {missingFile.Item1.Bolded().InItalics()} " +
                               $"for System: {missingFile.Item2.GetType().Name}");
            }   
        }
        return allFilesFound;
    }
}