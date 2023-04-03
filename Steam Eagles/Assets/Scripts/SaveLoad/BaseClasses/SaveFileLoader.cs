namespace SaveLoad
{
    public abstract class SaveFileLoader<T>
    {
     
        /// <summary>
        /// attempt to load the save state from the given path, return true if successful
        /// </summary>
        /// <param name="savePath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract bool LoadSaveState(string savePath);
    }
}