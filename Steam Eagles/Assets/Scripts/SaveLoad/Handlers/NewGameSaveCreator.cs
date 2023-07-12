using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoreLib;
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


        public List<INewGameSaveFileCreator> creators = new List<INewGameSaveFileCreator>();
        public NewGameSaveCreator(bool debug)
        {
            var types = ReflectionUtils.GetConcreteTypes<INewGameSaveFileCreator>();
            var instances = ReflectionUtils.GetConcreteInstances<INewGameSaveFileCreator>();
            if (debug)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Found Types inheriting from INewGameSaveFileCreator:");
                foreach (var type in types)
                {
                    sb.AppendLine($"{type.FullName}");
                }
                Debug.Log(sb.ToString());
            }
            creators = instances;
            this.debug = true;
        }
        
        public void CreateNewGameSave(string savePath)
        {
            if (!savePath.Contains(Application.persistentDataPath))
            {
                savePath = $"{Application.persistentDataPath}/{savePath}";
            }
            Debug.Log($"Initializing New Game to save path: \n{savePath.Bolded().InItalics()}");
            // PersistenceManager.Instance.Initialize(savePath);
            Directory.CreateDirectory(savePath);
            foreach (var gameSaveFileCreator in creators)
            {
                gameSaveFileCreator.CreateNewSaveFile(savePath);
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