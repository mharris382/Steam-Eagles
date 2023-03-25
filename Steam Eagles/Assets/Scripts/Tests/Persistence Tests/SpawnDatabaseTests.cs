using System.Collections;
using System.Collections.Generic;
using CoreLib.SaveLoad;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SpawnDatabaseTests
{
    [Test]
    public void SpawnDatabaseExists()
    {
        var spawnDB = SpawnDatabase.Instance;
        Assert.IsNotNull(spawnDB);
    }
  
    
    [Test]
    public void SpawnDatabaseHasSpawnPoints()
    {
        var spawnDB = SpawnDatabase.Instance;
        
    }
}
