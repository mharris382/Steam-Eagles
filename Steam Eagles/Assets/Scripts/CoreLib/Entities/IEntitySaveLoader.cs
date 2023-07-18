using Cysharp.Threading.Tasks;

namespace CoreLib.MyEntities
{
    public interface IEntitySaveLoader
    {
        UniTask<bool> SaveEntity(EntityHandle handle);
        UniTask<bool> LoadEntity(EntityHandle handle);
    }
}