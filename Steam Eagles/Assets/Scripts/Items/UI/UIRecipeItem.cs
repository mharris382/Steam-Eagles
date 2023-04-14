using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Items.UI
{
    public class UIRecipeItem : MonoBehaviour
    {
        [SerializeField]
        private UIRecipeStack stack;

        [Required]
        public RectTransform stackParent;
        
        
        public Image icon;

        [OnValueChanged(nameof(OnValueChanged))]
        public Recipe recipe;

        void OnValueChanged(Recipe r)
        {
            
        }
    }


    [Serializable]
    public class UIRecipeStack
    {

        public int sizeAvailable = 14;
        public string formatString = "{0} / <size={2}>{1}</size>";
        
        public Color validColor = Color.black;
        public Color invalidColor = Color.red;
        
        public bool DisplayStack(TextMeshProUGUI text, int amountNeeded, int amountAvailable)
        {
            string s = string.Format(formatString, amountNeeded, amountAvailable, sizeAvailable);
            var color = amountAvailable >= amountNeeded ? validColor : invalidColor;
            text.text = s;
            text.color = color;
            return amountAvailable >= amountNeeded;
        }
    }
}