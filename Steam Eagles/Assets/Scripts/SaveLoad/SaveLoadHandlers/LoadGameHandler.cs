using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SaveLoad
{
    public class LoadGameHandler
    {
        public Dictionary<LoadOrderAttribute, List<IGameLoader>> loadHandlers;

        private List<LoadOrderAttribute> loadOrderAttributes;
        public LoadGameHandler(bool debug)
        {
            loadHandlers = new Dictionary<LoadOrderAttribute, List<IGameLoader>>();
            loadOrderAttributes = new List<LoadOrderAttribute>();
            var handlers = ReflectionUtils.GetConcreteInstances<IGameLoader>();
            foreach (var handler in handlers)
            {
                var loadOrderAttribute = handler.GetType().GetCustomAttributes(typeof(LoadOrderAttribute), false)[0] as LoadOrderAttribute;
                if(loadOrderAttribute == null)loadOrderAttribute = new LoadOrderAttribute(0);
                
                if (loadHandlers.ContainsKey(loadOrderAttribute))
                {
                    loadHandlers[loadOrderAttribute].Add(handler);
                }
                else
                {
                    loadHandlers.Add(loadOrderAttribute, new List<IGameLoader> {handler});
                    loadOrderAttributes.Add(loadOrderAttribute);
                }
            }
            loadOrderAttributes.Sort();

            if (!debug) return;
            Debug.Log("Found Load Handlers:");
            StringBuilder sb = new StringBuilder();
            foreach (var load in loadOrderAttributes)
            {
                sb.AppendLine($"Load Order Group: {load.Order}");
                foreach (var handler in loadHandlers[load])
                {
                    sb.AppendLine($"\t{handler.GetType().FullName}");
                }
            }
            Debug.Log(sb.ToString());
        }

        public void LoadGame(string path)
        {
            foreach (LoadOrderAttribute loadOrderAttribute in loadOrderAttributes)
            {
                foreach (var loadHandler in loadHandlers[loadOrderAttribute])
                {
                    loadHandler.LoadGame(path);
                }
            }
        }
    }
}