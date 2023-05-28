using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public interface ISaveLoaderSystem
{
    UniTask<bool> SaveGameAsync(string savePath);
    UniTask<bool> LoadGameAsync(string loadPath);

    bool IsSystemOptional() => false;
    IEnumerable<(string name, string ext)> GetSaveFileNames();
}

public interface IJsonSaveLoaderSystem : ISaveLoaderSystem
{
    string GetJsonFileName();
    Type GetJsonFileType();
}