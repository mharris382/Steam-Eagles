using Cysharp.Threading.Tasks;

namespace CoreLib.Entities
{
    public interface IEntitySaveLoader
    {
        UniTask<bool> SaveEntity(EntityHandle handle);
        UniTask<bool> LoadEntity(EntityHandle handle);
    }
}