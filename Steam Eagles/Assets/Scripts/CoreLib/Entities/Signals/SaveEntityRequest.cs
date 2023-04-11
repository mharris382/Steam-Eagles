namespace CoreLib.Signals
{
    public struct SaveEntityRequest
    {
        public string EntityID { get; }

        public SaveEntityRequest(string entityID)
        {
            EntityID = entityID;
        }
    }
}