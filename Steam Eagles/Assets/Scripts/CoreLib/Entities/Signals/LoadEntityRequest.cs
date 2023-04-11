namespace CoreLib.Signals
{
    public struct LoadEntityRequest
    {
        public string EntityID { get; }

        public LoadEntityRequest(string entityID)
        {
            EntityID = entityID;
        }
    }
}