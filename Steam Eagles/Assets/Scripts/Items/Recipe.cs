﻿using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Steam Eagles/Items/Recipe")]
    public class Recipe : ScriptableObject
    {
        [TableList]
        public List<ItemStack> components;
    }
}