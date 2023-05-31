using System.IO;

namespace CoreLib.Entities
{
    public class EntitySavePath
    {
        private readonly GlobalSavePath _savePath;

        public EntitySavePath(GlobalSavePath savePath)
        {
            _savePath = savePath;
        }
        
        public bool HasSavePath
        {
            get
            {
                bool result = _savePath.HasSavePath;
                if (result)
                {
                    string entitySubFolder = $"{_savePath.FullSaveDirectoryPath}/Entities";
                    if (!Directory.Exists(entitySubFolder)) Directory.CreateDirectory(entitySubFolder);
                }
                return result;
            }
        }
        public string FullSaveDirectoryPath => $"{_savePath.FullSaveDirectoryPath}/Entities";
    }
}