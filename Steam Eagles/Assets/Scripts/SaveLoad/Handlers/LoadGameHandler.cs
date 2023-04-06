using System.Collections.Generic;
using System.Text;
using CoreLib;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
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
                var loadOrderAttribute = handler.GetType().GetCustomAttributes(typeof(LoadOrderAttribute), false)[0] as LoadOrderAttribute ??
                                         new LoadOrderAttribute(0);

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


    public class AsyncLoadGameHandler
    {
        private readonly List<LoadOrderAttribute> _loadOrderAttributes;
        private readonly Dictionary<LoadOrderAttribute, List<IAsyncGameLoader>> _loadHandlers;

        public AsyncLoadGameHandler(bool debug)
        {
            _loadHandlers = new Dictionary<LoadOrderAttribute, List<IAsyncGameLoader>>();
            _loadOrderAttributes = new List<LoadOrderAttribute>();
            var handlers = ReflectionUtils.GetConcreteInstances<IAsyncGameLoader>();
            foreach (var asyncGameLoader in handlers)
            {
                var loadOrderAttribute = asyncGameLoader.GetType().GetCustomAttributes(typeof(LoadOrderAttribute), false)[0] as LoadOrderAttribute ?? new LoadOrderAttribute(0);
                if (!_loadHandlers.TryGetValue(loadOrderAttribute, out handlers))
                {
                    handlers = new List<IAsyncGameLoader>();
                    _loadHandlers.Add(loadOrderAttribute, handlers);
                    _loadOrderAttributes.Add(loadOrderAttribute);
                }
                handlers.Add(asyncGameLoader);
            }
            
            _loadOrderAttributes.Sort();
            Debug.Assert(_loadOrderAttributes.Count > 0, "No LOADER GROUPS FOUND!");
            if (!debug) return;
            Debug.Log("Found Load Handlers:");
            StringBuilder sb = new StringBuilder();
            foreach (var load in _loadOrderAttributes)
            {
                sb.AppendLine($"Load Order Group: {load.Order}");
                foreach (var handler in _loadHandlers[load])
                {
                    sb.AppendLine($"\t{handler.GetType().FullName}");
                }
            }
            Debug.Log(sb.ToString());
        }



        public async UniTask LoadAsync(string path)
        {
            foreach (var loadOrderAttribute in _loadOrderAttributes)
            {
                Debug.Log($"Starting Load Group: {loadOrderAttribute.Order}");
                
                var loaders = _loadHandlers[loadOrderAttribute];
#if false
                foreach (var loader in loaders)
                {
                    var success = await loader.LoadGameAsync(path);
                    if (!success) 
                        Debug.LogError($"Loader {loader.GetType().Name} Failed at path {path}!");
                }
#else
                var results = await UniTask.WhenAll(loaders.Select(t => t.LoadGameAsync(path)));
#endif
                bool allResultsSuccess = true;
                for (int i = 0; i < results.Length; i++)
                {
                    var result = results[i];
                    if (!result)
                    {
                        allResultsSuccess = false;
                        Debug.Log($"Failed to load: {loaders[i].GetType().Name.Bolded()}");
                    }
                }

                if (!allResultsSuccess)
                {
                    Debug.LogError($"Loading Failed within load group: {loadOrderAttribute.Order}");
                    return;
                }
            }
        }
    }
}