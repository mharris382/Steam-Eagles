namespace CoreLib
{
    public class PersistenceState
    {
        public Action CurrentAction { get;  set; }
        public enum Action
        {
            NONE,
            LOADING,
            SAVING
        }

        public override string ToString()
        {
            return CurrentAction.ToString().ToLower();
        }
    }

}