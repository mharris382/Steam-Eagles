namespace Tools
{
    /// <summary>
    /// used to update UI when the tool mode changes
    /// </summary>
    public interface IModeNameListener
    {
        public void DisplayModeName(string modeName);
    }
}