using Cysharp.Threading.Tasks;

namespace SaveLoad
{
    public interface IGameLoader
    {
        void LoadGame(string savePath);
    }
    
    
    public interface IAsyncGameLoader
    {
        UniTask<bool> LoadGameAsync(string savePath);
    }
}