namespace CoreLib.Entities
{
    /// <summary>
    /// any class implementing this interface will implicitly be
    /// created and used by the EntityLoadHandler which handles
    /// loading and saving of specific entities
    /// <seealso cref="IEntitySaveLoader"/> 
    /// </summary>
    public interface IEntityTypeSaveLoader : IEntitySaveLoader
    {
        public EntityType GetEntityType();
        
    }
}