using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game;
using NUnit.Framework;
using SaveLoad;
using SaveLoad.CoreSave;
using UnityEngine;
using UnityEngine.TestTools;

public class CreateNewGameSaveFileTest
{
    public const string EXPECTED_SAVE_PATH = "Test New Game Save";

    private GameObject _gameManager;
    [SetUp]
    public void SetUp()
    {
        _gameManager = new GameObject("GM", typeof(GameManager));
        var path = $"{Application.persistentDataPath}/{EXPECTED_SAVE_PATH}";
        if (Directory.Exists(path))
        {
            Debug.Log($"Deleting full {path}");
            Directory.Delete(path, true);
        }
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.Destroy(_gameManager.gameObject);
    }

    private static string GetTestSavePath() => $"{Application.persistentDataPath}/{EXPECTED_SAVE_PATH}";


    [Test]
    public void CanCreateNewGameSaveDirectory()
    {
        var newGameCreator = new NewGameSaveCreator(true);
        Assert.IsFalse(Directory.Exists(GetTestSavePath()));
        newGameCreator.CreateNewGameSave(GetTestSavePath());
        Assert.IsTrue(Directory.Exists(GetTestSavePath()));
    }
    
    [Test]
    public void NewGameSaveContainsCoreSaveData()
    {
        var newGameCreator = new NewGameSaveCreator(true);
        var savePath = GetTestSavePath();
        newGameCreator.CreateNewGameSave(savePath);
        var files = Directory.GetFiles(savePath).ToList();
        
        Assert.IsTrue(files.Count > 0);
        
        var expectedFiles = new string[]
        {
            GetSaveFilePath<CoreSaveData>()
        };
        HashSet<string> actualPaths = new HashSet<string>();
        foreach (var file in files)
        {
            actualPaths.Add(file);
        }
        foreach (var expectedPath in expectedFiles)
        {
            Assert.IsTrue(actualPaths.Contains(expectedPath));
            string json = File.ReadAllText(expectedPath);
            var coreSaveData = JsonUtility.FromJson<CoreSaveData>(json);
            Assert.IsNotNull(coreSaveData);
        }
    }
    
    
    
    public string GetSaveFilePath<T>() => $"{GetTestSavePath()}\\{typeof(T).Name}.json";
}