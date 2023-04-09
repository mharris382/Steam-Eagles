using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.InputSystem;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{
    public abstract class InputIconSetBasicSO : ScriptableObject
    {
        protected enum SearchPattern { One, All };

        public string iconSetName;

        public Color deviceDisplayColor;

        public InputSpriteData unboundData = new InputSpriteData("Unbound Sprite", null, "");
        public InputSpriteData fallbackData = new InputSpriteData("Fallback Sprite", null, "FallbackSprite");

        public List<CustomInputContextIcon> customContextIcons = new List<CustomInputContextIcon>();

        public abstract string controlSchemeName { get; }




        public void OverrideStylesInStyleSheet()
        {
            List<TMP_InputStyleHack.StyleStruct> styleUpdates = GetStyleUpdates();

            TMP_InputStyleHack.UpdateStyles(styleUpdates);
        }

        protected List<TMP_InputStyleHack.StyleStruct> GetEmptyStyleUpdates(List<TMP_InputStyleHack.StyleStruct> styleUpdates)
        {

            for (int i = 0; i < styleUpdates.Count; i++)
            {
                styleUpdates[i] = new TMP_InputStyleHack.StyleStruct(styleUpdates[i].name, "", "");
            }

            return styleUpdates;
        }


        public List<TMP_InputStyleHack.StyleStruct> GetStyleUpdates()
        {

            List<TMP_InputStyleHack.StyleStruct> styleUpdates = new List<TMP_InputStyleHack.StyleStruct>();

            List<InputStyleData> inputStyles;

            string inputIconsOpeningTag = InputIconsManagerSO.Instance.openingTag;
            string inputIconsClosingTag = InputIconsManagerSO.Instance.closingTag;

            if (controlSchemeName == InputIconsManagerSO.Instance.controlSchemeName_Keyboard)
                inputStyles = InputIconsManagerSO.Instance.inputStyleKeyboardDataList;
            else
                inputStyles = InputIconsManagerSO.Instance.inputStyleGamepadDataList;

            for (int i=0; i<inputStyles.Count; i++)
            {
                if (inputStyles[i] == null)
                    continue;


                string style = InputIconsManagerSO.Instance.GetCustomStyleTag(inputStyles[i]);
                style = inputIconsOpeningTag + style + inputIconsClosingTag;
                styleUpdates.Add(new TMP_InputStyleHack.StyleStruct(inputStyles[i].bindingName, style, ""));
              
            }


            return styleUpdates;
        }

        public bool HasSprite(string bindingTag)
        {
            List<InputSpriteData> spriteData = GetAllInputSpriteData();
            for (int i = 0; i < spriteData.Count; i++)
            {
                if (spriteData[i].textMeshStyleTag.ToUpper() == bindingTag.ToUpper())
                {
                    if (spriteData[i].sprite == null)
                        return false;
                    else
                        return true;
                }
                   
            }

            return false;
        }

        public abstract void TryGrabSprites();
        public abstract List<InputSpriteData> GetAllInputSpriteData();

        protected Sprite GetSpriteFromList(List<Sprite> spriteList, string[] spriteTags, SearchPattern pattern)
        {
            for (int i = 0; i < spriteList.Count; i++)
            {
                int count = 0;
                for (int j = 0; j < spriteTags.Length; j++)
                {

                    if (spriteList[i].name.ToUpper().Contains(spriteTags[j].ToUpper()))
                    {
                        count++;
                        if (pattern == SearchPattern.One)
                            return spriteList[i];
                    }
                }
                if (pattern == SearchPattern.All && count >= spriteTags.Length)
                    return spriteList[i];
            }

            string s = "";
            for (int i = 0; i< spriteTags.Length; i++)
                s += spriteTags[i].ToString()+" ";

            //InputIconsLogger.Log("Sprite not found, "+ s);
            return null;
        }

        protected List<Sprite> GetSpritesAtPath(string path)
        {
            List<Sprite> sprites = new List<Sprite>();
#if UNITY_EDITOR
            string[] guids = AssetDatabase.FindAssets( "t:Sprite", new string[] {path} );


            foreach (string o in guids)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(o).ToString();
                //Debug.Log(spritePath);
                Sprite s = (Sprite)AssetDatabase.LoadAssetAtPath(spritePath, typeof(Sprite));
                sprites.Add(s);
            }
#endif
            return sprites;
        }
    }

    [System.Serializable]
    public struct CustomInputContextIcon
    {

        public Sprite customInputContextSprite;
        public string textMeshStyleTag;
    }

    [System.Serializable]
    public struct InputSpriteData
    {
        [HideInInspector]
        private string buttonName;
        public string textMeshStyleTag;
        public Sprite sprite;


        public InputSpriteData(string buttonName, Sprite aSprite, string tag)
        {
            this.buttonName = buttonName;
            sprite = aSprite;
            textMeshStyleTag = tag;
        }

        public string GetButtonName()
        {
            if (buttonName == "")
                return textMeshStyleTag;
            return buttonName;
        }

    }
}