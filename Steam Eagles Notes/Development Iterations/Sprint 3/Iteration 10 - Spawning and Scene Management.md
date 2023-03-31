%%
[[Sprint 3]] #iteration #unity
%%
# Iteration 10: Spawning and Scene Management
Previous Iteration: [[Iteration 09 - Climbing Movement]]
Next Iteration: [[Iteration 11 - Persistence Manager]]


## Goal

### Purpose
so recently I ended up in a conversation with some other developers about the best way to handle spawn positions.  [I proposed a solution](https://pastebin.com/c9nVQkje) to their problem which I ended noticing could be a good solution to my own problem with spawning (which was not working). 

### Hypothesis
use spawn database to handle resolving spawn positions

----
## Result

## `SpawnDB.cs`
```cs
    public class SpawnDatabase : ScriptableObject
{
    private static SpawnDatabase _instance;

    public static SpawnDatabase Instance => _instance ??= Resources.Load<SpawnDatabase>("Spawn Database");

    [SerializeField]
    private List<SpawnPoint> spawnPoints;
    
    private Dictionary<string, Transform> _dynamicSpawnPoints = new Dictionary<string, Transform>();

    [System.Serializable]
    public class SpawnPoint
    {
        public string characterName;
        public Vector3 defaultSpawnPosition;
    }
 
    [System.Serializable]
    public class SavedSpawnPoints
    {
        public List<SavedSpawnPoint> savedSpawnPoints;
    }
    
    [System.Serializable]
    public class SavedSpawnPoint
    {
        public string characterName;
        public Vector3 spawnPosition;
    }
 
    public Vector3 GetDefaultSpawnPointForScene(string characterName)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                return spawnPoint.defaultSpawnPosition;
            }
        }
        Debug.LogError($"Level Name {characterName} does not have defined spawn position, please add one", this);
        return Vector3.zero;
    }

    
    public void RegisterDynamicSpawn(string character, Transform spawnPoint)
    {
        if (_dynamicSpawnPoints.ContainsKey(character))
        {
            _dynamicSpawnPoints[character] = spawnPoint;
        }
        else
        {
            _dynamicSpawnPoints.Add(character, spawnPoint);
        }
    }
    
    public void RemoveDynamicSpawn(string character)
    {
        if (_dynamicSpawnPoints.ContainsKey(character))
        {
            _dynamicSpawnPoints.Remove(character);
        }
    }
    
    public Vector3 GetSpawnPointForScene(string characterName, string persistentSaveDataPath)
    {
        Vector3 spawnPoint;
        
        if (_dynamicSpawnPoints.ContainsKey(characterName))
        {
            if(_dynamicSpawnPoints[characterName] != null)
                return _dynamicSpawnPoints[characterName].position;
            else
                _dynamicSpawnPoints.Remove(characterName);
        }
        
        if(TryLoadSpawnPoint(characterName, persistentSaveDataPath, out spawnPoint))
            return spawnPoint;
        
        return GetDefaultSpawnPointForScene(characterName);
    }
    
    private bool TryLoadSpawnPoint(string characterName, string persistentSaveDataPath, out Vector3 spawnPoint)
    {
        spawnPoint = Vector3.zero;
        string dirPath = Application.persistentDataPath + persistentSaveDataPath;

        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
        
        string filepath =  dirPath + (dirPath.EndsWith("/") ? "SpawnPoints.dat" : "/SpawnPoints.dat");
        if (!File.Exists(filepath))
        {
            return false;
        }
        using (FileStream file = (File.Exists(filepath) ? File.Open(filepath, FileMode.Open) : File.Create(filepath)))
        {
            if(file.Length == 0)
                return false;
            SavedSpawnPoints savedSpawnPoints = (SavedSpawnPoints) new BinaryFormatter().Deserialize(file);
            foreach (var savedSpawnPoint in savedSpawnPoints.savedSpawnPoints)
            {
                if (savedSpawnPoint.characterName == characterName)
                {
                    spawnPoint = savedSpawnPoint.spawnPosition;
                    return true;
                }
            }
        }
        return false;
    }


    public bool HasDefaultSpawnPosition(string characterName)
    {
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                return true;
            }
        }
        return false;
    }
    public void UpdateDefaultSpawnPoint(string characterName, Vector3 spawnPosition)
    {
        if (!Application.isEditor || Application.isPlaying)
        {
            Debug.LogError("Default Spawn points can only be called in editor", this);
            return;
        }
        
        foreach (var spawnPoint in spawnPoints)
        {
            if (spawnPoint.characterName == characterName)
            {
                spawnPoint.defaultSpawnPosition = spawnPosition;
                return;
            }
        }
        Debug.Log("Adding new spawn point for character " + characterName, this);
        spawnPoints.Add(new SpawnPoint()
        {
            characterName = characterName,
            defaultSpawnPosition = spawnPosition
        });
    }
}
```



----

## `SpawnUpdater.cs`
i think the point of this class was to register a spawn point that would override the saved spawn position in the scene.  Now I'm not 100% sure about this script because I think the way it is now, it will interupt the initial spawn when a game is loaded.
```cs
public class SpawnUpdater : MonoBehaviour
    {
        public string characterName;

        private void Awake()
        {
            SpawnDatabase.Instance.RegisterDynamicSpawn(characterName, transform);
        }

        private void OnDestroy()
        {
            SpawnDatabase.Instance.RemoveDynamicSpawn(characterName);
        }
    }
```



## `PersistenceManager.cs`
```cs
public class PersistenceManager : Singleton<PersistenceManager>  
{  
    public string SaveDirectoryPath { get; private set; }  
}
```
so basically the idea here was to define a save directory path for this particular playthrough.  The path would be initially defined when a new game is started.

## Reflection

OK so the process for determining where to spawn the character is as follows:

1. Check if this character has a dynamic spawn point in the scene?
	1. If true -> use dynamic spawn position
2. IF false -> load the current spawn save file and check if character spawn position was saved
3. If false -> use the initial spawn position in the spawn database


### What was learned or accomplished?
This method does work, but I'm not really sure if it is necessary.  I think there may be something useful here, like using this sort of system to find spawn locations by name.

That functionality could be very useful with a console window, combined with custom commands (example `respawn [ENTITY NAME] [LOCATION NAME] [SUB-LOCATION NAME (option)]`)
- respawn is the command
- entity name is the entity we wish to teleport
- location name is the .... wait this does not account for the airship being able to move around.... ok nvm come back to this

The persistence manager is an important entry point for the game, currently does not do anything, but it should be setup next I think.


### Where to go now?

Entry point for the persistence manager