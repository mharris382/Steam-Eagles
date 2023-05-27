using Cysharp.Threading.Tasks;

public interface ISaveLoaderSystem
{
    UniTask<bool> SaveGameAsync(string savePath);
    UniTask<bool> LoadGameAsync(string loadPath);
}