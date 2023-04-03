using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

namespace SaveLoad
{
    public class NewGameSaveCreator
    {
        private readonly bool debug;

        public class NewGameSaveCreatorTypeGroup : IComparable<NewGameSaveCreatorTypeGroup>
        {
            public int groupOrder;
            public List<INewGameSaveFileCreator> creators = new List<INewGameSaveFileCreator>();

            public int CompareTo(NewGameSaveCreatorTypeGroup other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return groupOrder.CompareTo(other.groupOrder);
            }
        }


        private Dictionary<int, NewGameSaveCreatorTypeGroup> groups;
        public List<INewGameSaveFileCreator> creators = new List<INewGameSaveFileCreator>();
        public NewGameSaveCreator(bool debug)
        {
            groups = new Dictionary<int, NewGameSaveCreatorTypeGroup>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(a => a.GetTypes()).Where(t => t.IsConcrete()).ToArray();
            creators = new List<INewGameSaveFileCreator>(types.Length);
            foreach (var type in types)
            {
                creators.Add(Activator.CreateInstance(type) as INewGameSaveFileCreator);
                if(debug)Debug.Log($"Found {type.Name}");
            }
                
            this.debug = true;
        }
        
        public void CreateNewGameSave(string savePath)
        {
            foreach (var group in groups.Values.OrderBy(t => t.groupOrder))
            {
                Debug.Log($"Saving Group {group.groupOrder}");
                foreach (var creator in group.creators)
                {
                    Debug.Log($"Saving {creator.GetType().Name}");
                    creator.CreateNewSaveFile(savePath);
                }
            }
        }
    }
    
    public static class TypeExtensions
    {
        public static bool IsConcrete(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }
    }
}