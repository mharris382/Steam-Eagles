namespace CoreLib
{
    public struct SaveGameRequestedInfo
    {
        public string savePath;

        public SaveGameRequestedInfo(string savePath)
        {
            this.savePath = savePath;
        }
    }

    public struct LoadGameRequestedInfo
    {
        public string loadPath;

        public LoadGameRequestedInfo(string loadPath)
        {
            this.loadPath = loadPath;
        }
    }
}