namespace SaveLoad
{
    public class SaveFailedException : System.Exception
    {
        public SaveFailedException(string message) : base(message)
        {
        }
    }
}