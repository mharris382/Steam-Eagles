namespace Buildings
{
    /// <summary>
    /// note this is the entity ID specifically for objects inside buildings
    /// </summary>
    public interface IEntityID
    {
        public string GetEntityGUID();
    }
}