namespace CoreLib
{
    [System.Obsolete("Call SaveLoader directly")]
    public struct SaveGameRequestedInfo
    {
        public string savePath;

        public SaveGameRequestedInfo(string savePath)
        {
            this.savePath = savePath;
        }
    }

    [System.Obsolete("Call SaveLoader directly")]
    public struct LoadGameRequestedInfo
    {
        public string loadPath;

        public LoadGameRequestedInfo(string loadPath)
        {
            this.loadPath = loadPath;
        }
    }
}