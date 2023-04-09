using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using static InputIcons.InputIconsUtility;

namespace InputIcons
{

    [CreateAssetMenu(fileName = "InputIconsManager", menuName = "Input Icon Set/Input Icons Manager", order = 504)]
    public class InputIconsManagerSO : ScriptableObject
    {
        private static InputIconsManagerSO instance;
        public static InputIconsManagerSO Instance
        {
            get
            {
                if (instance != null)
                    return instance;
                else
                {
                    InputIconsManagerSO iconManager = Resources.Load("InputIcons/InputIconsManager") as InputIconsManagerSO;
                    if (iconManager)
                    {
                        instance = iconManager;
                    }
                    

                    return instance;
                }
            }
            set => instance = value;
        }

        public InputIconSetConfiguratorSO iconSetConfiguratorSO;

        private InputDevice currentInputDevice;

        public List<InputActionAsset> usedActionAssets;

        [Tooltip("The name of the keyboard control scheme in the Input Action Asset")]
        public string controlSchemeName_Keyboard = "Keyboard And Mouse";
        [Tooltip("The name of the gamepad control scheme in the Input Action Asset")]
        public string controlSchemeName_Gamepad = "Gamepad";

        [Header("Display Options")]
        [Tooltip("If true, will display 'WASD or Arrowkeys' in <style=Move> for example. If false, will only display WASD (or the first option set in the Input Action Asset).")]
        public bool showAllInputOptionsInStyles = false;
        public string openingTag = "";
        public string closingTag = "";
        public string multipleInputsDelimiter = " <size=80%>or</size> ";
        public string compositeInputDelimiter = ", ";

        public string textDisplayForUnboundActions = "Undefined";
        public enum TextDisplayLanguage { EnglishOnly, SystemLanguage };
        public TextDisplayLanguage textDisplayLanguage = TextDisplayLanguage.EnglishOnly;

        public List<ActionRenamingStruct> actionNameRenamings = new List<ActionRenamingStruct>();
        [System.Serializable]
        public struct ActionRenamingStruct
        {
            public string originalString;
            public string outputString;
        }

        public enum TextUpdateOptions { SearchAndUpdate, ViaInputIconsTextComponents };
        [Header("Text update options")]
        public TextUpdateOptions textUpdateOptions = TextUpdateOptions.SearchAndUpdate;
        private static readonly List<InputIconsText> activeTexts = new List<InputIconsText>();



        public enum RebindBehaviour { OverrideExisting, CancelOverrideIfBindingAlreadyExists };
        [Header("Rebinding Options")]
        public RebindBehaviour rebindBehaviour = RebindBehaviour.OverrideExisting;
        public bool loadAndSaveInputBindingOverrides = true;

        public enum DisplayType { Sprites, Text, TextInBrackets };
        public DisplayType displayType = DisplayType.Sprites;

        public delegate void OnControlsChanged(InputDevice inputDevice);
        public static OnControlsChanged onControlsChanged;

        public delegate void OnBindingsChanged();
        public static OnBindingsChanged onBindingsChanged;

        public List<InputStyleData> inputStyleKeyboardDataList;
        public List<InputStyleData> inputStyleGamepadDataList;

        private string lastKeyboardLayout = "";
        private InputIconSetBasicSO lastGamepadIconSet = null;

        public string TEXTMESHPRO_SPRITEASSET_FOLDERPATH = "Assets/TextMesh Pro/Resources/Sprite Assets/";

        public bool loggingEnabled = true;

        private static bool listeningForDeviceChange = false;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunOnStart()
        {
            if (UnityEditor.EditorSettings.enterPlayModeOptionsEnabled)
            {
                Instance.Initialize();
            }
        }
#endif

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            Instance = this;
            Instance.Initialize();

#if STEAMWORKS_NET
            ScriptableObject.CreateInstance<InputIconsSteamworksExtensionSO>();
#endif

        }

        public void Initialize()
        {
            LoadUserRebinds();
            if (!listeningForDeviceChange)
            {
                InputSystem.onActionChange += HandleDeviceChange;
                listeningForDeviceChange = true;
                InputIconsLogger.Log("listening for on device change event");
            }
               
        }

        public static void HandleInputBindingsChanged()
        {
            Instance.CreateInputStyleData();
            onBindingsChanged?.Invoke();

            InputIconsUtility.RefreshAllTMProUGUIObjects();
        }


        public static void HandleDeviceChange(object obj, InputActionChange change)
        {
   
            if (change != InputActionChange.ActionPerformed)
                return;

            InputDevice device = ((InputAction)obj).activeControl.device;
            InputDevice currentInputDevice = GetCurrentInputDevice();
            if ((currentInputDevice is Mouse && device is Keyboard)
                || (currentInputDevice is Keyboard && device is Mouse)
                || (currentInputDevice == device))
                return;

            if (device == null)
                return;

            Instance.currentInputDevice = device;

            InputIconSetConfiguratorSO.UpdateCurrentIconSet();

            Instance.UpdateInputStyleData();
            UpdateTMProStyleSheetWithUsedPlayerInputs();
            onControlsChanged?.Invoke(GetCurrentInputDevice());

            InputIconsUtility.RefreshAllTMProUGUIObjects();  
        }


        public void CreateInputStyleData()
        {
            if (InputIconSetConfiguratorSO.Instance == null)
            {
                InputIconsLogger.LogWarning("InputIconSetConfigurator Instance was null, please try again.");
                return;
            }

            CreateKeyboardInputStyleData(InputIconSetConfiguratorSO.Instance.keyboardIconSet.iconSetName);
            inputStyleKeyboardDataList = GetCleanedUpStyleList(inputStyleKeyboardDataList);


            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();
            InputIconSetBasicSO gamepadIconSet = lastGamepadIconSet;

            if(iconSet!=null)
            {
                if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
                    gamepadIconSet = iconSet;
            }

            if (gamepadIconSet == null)
                gamepadIconSet = InputIconSetConfiguratorSO.Instance.xBoxIconSet;

            CreateGamepadInputStyleData(gamepadIconSet.iconSetName);
            inputStyleGamepadDataList = GetCleanedUpStyleList(inputStyleGamepadDataList);

            UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        public void UpdateInputStyleData()
        {
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();
            if (iconSet.GetType() == typeof(InputIconSetKeyboardSO))
            {
                if(Keyboard.current==null)
                {
                    CreateKeyboardInputStyleData(iconSet.iconSetName);
                    inputStyleKeyboardDataList = GetCleanedUpStyleList(inputStyleKeyboardDataList);
                }
                else if(Keyboard.current.keyboardLayout != lastKeyboardLayout)
                {
                    CreateKeyboardInputStyleData(iconSet.iconSetName);
                    inputStyleKeyboardDataList = GetCleanedUpStyleList(inputStyleKeyboardDataList);
                }
            }

            if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
            {
                if(iconSet != lastGamepadIconSet)
                {
                    CreateGamepadInputStyleData(iconSet.iconSetName);
                    inputStyleGamepadDataList = GetCleanedUpStyleList(inputStyleGamepadDataList);
                }
            }

            UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        public void CreateKeyboardInputStyleData(string deviceDisplayName)
        {
            if(Keyboard.current!=null)
                lastKeyboardLayout = Keyboard.current.keyboardLayout;

            inputStyleKeyboardDataList = InputIconsUtility.CreateInputStyleData(usedActionAssets, controlSchemeName_Keyboard, deviceDisplayName);
        }

        public void CreateGamepadInputStyleData(string deviceDisplayName)
        {
            lastGamepadIconSet = InputIconSetConfiguratorSO.GetIconSet(deviceDisplayName);
           
            inputStyleGamepadDataList = InputIconsUtility.CreateInputStyleData(usedActionAssets, controlSchemeName_Gamepad, deviceDisplayName);
        }

     
        /// <summary>
        /// Removes bindings which are only available in one of the style lists. E.g. if Jump/3 is only available for keyboard, remove it,
        /// since it could not be displayed when using a gamepad
        /// </summary>
        private List<InputStyleData> GetCleanedUpStyleList(List<InputStyleData> styleList)
        {

            for (int i = styleList.Count-1; i >= 0; i--) //remove empty entries
            {
                if (styleList[i].bindingName == null)
                {
                    styleList.RemoveAt(i);
                }
            }

            //setup the single style tag fields
            for(int i=0; i<styleList.Count; i++)
            {
                styleList[i].inputStyleString_singleInput = styleList[i].inputStyleString;
                styleList[i].humanReadableString = styleList[i].humanReadableString.ToUpper();
                styleList[i].humanReadableString_singleInput= styleList[i].humanReadableString;
            }

            
            List<string> combinedBindingNames = new List<string>();
            bool combinedABinding;

            for(int i=0; i < styleList.Count; i++)
            {
                combinedABinding = false;
                for (int j = 0; j < styleList.Count; j++)
                {
                    if (j == i)
                        continue;

                    if (styleList[j].bindingName == styleList[i].bindingName
                        && (styleList[i].isComposite || styleList[i].isPartOfComposite || styleList[j].isComposite || styleList[j].isPartOfComposite))
                    {
                        //combine composites and part of composites
                        styleList[i].inputStyleString += multipleInputsDelimiter + styleList[j].inputStyleString;
                        styleList[i].humanReadableString += multipleInputsDelimiter + styleList[j].humanReadableString;
                        

                        styleList.RemoveAt(j);
                        j--;

                        continue;
                    }
                    

                    if(!styleList[i].isComposite && !styleList[i].isPartOfComposite)
                    {
                        if(styleList[j].bindingName == styleList[i].bindingName
                            && !combinedBindingNames.Contains(styleList[i].bindingName))
                        {
                            //combine single button bindings (e.g. if there are multiple bindings to a jump action)
                            styleList[i].inputStyleString += multipleInputsDelimiter + styleList[j].inputStyleString;
                            styleList[i].humanReadableString += multipleInputsDelimiter + styleList[j].humanReadableString;

                            combinedABinding = true;
                        }
                    }
                }

                if(combinedABinding)
                    combinedBindingNames.Add(styleList[i].bindingName);
            }

            for (int i = 0; i<styleList.Count; i++)
            {
                int c = 2;
                for (int j = 0; j < styleList.Count; j++)
                {
                    if (styleList[i].bindingName == styleList[j].bindingName) //make multiple binding names distinct by adding a counter at the end
                    {
                        if (i < j)
                        {
                            styleList[j].bindingName += "/" + c;
                            c++;
                        }
                    }

                    styleList[j].tmproReferenceText = "<style=" + styleList[j].bindingName + ">";
                }
            }
            
            return styleList;
        }

        public string GetCustomStyleTag(InputStyleData styleData)
        {
            if (showAllInputOptionsInStyles)
            {
                switch (displayType)
                {
                    case DisplayType.Sprites:
                        return styleData.inputStyleString;

                    case DisplayType.Text:
                        return styleData.humanReadableString;

                    case DisplayType.TextInBrackets:
                        return "[" + styleData.humanReadableString+"]";

                    default:
                        break;
                }
            }
            else
            {
                switch (displayType)
                {
                    case DisplayType.Sprites:
                        return styleData.inputStyleString_singleInput;

                    case DisplayType.Text:
                        return styleData.humanReadableString_singleInput;

                    case DisplayType.TextInBrackets:
                        return "[" + styleData.humanReadableString_singleInput + "]";

                    default:
                        break;
                }
            }

            return "";
        }

        public string GetCustomStyleTag(InputAction action, InputBinding binding)
        {
            if(showAllInputOptionsInStyles)
            {
                switch (displayType)
                {
                    case DisplayType.Sprites:
                        return GetSpriteStyleTag(action, binding);

                    case DisplayType.Text:
                        return GetHumanReadableString(action, binding);

                    case DisplayType.TextInBrackets:
                        return "[" + GetHumanReadableString(action, binding) + "]";

                    default:
                        break;
                }
            }
            else
            {
                switch (displayType)
                {
                    case DisplayType.Sprites:
                        return GetSpriteStyleTagSingle(action, binding);

                    case DisplayType.Text:
                        return GetHumanReadableStringSingle(action, binding);

                    case DisplayType.TextInBrackets:
                        return "[" + GetHumanReadableStringSingle(action, binding) + "]";

                    default:
                        break;
                }
            }
            
            return "";
        }

        public InputStyleData GetInputStyleData(string bindingName)
        {
            List<InputStyleData> styleList = inputStyleKeyboardDataList;
            InputIconSetBasicSO iconSet = InputIconSetConfiguratorSO.GetCurrentIconSet();

            if (iconSet.GetType() == typeof(InputIconSetGamepadSO))
            {
                styleList = inputStyleGamepadDataList;
            }

            for (int i = 0; i < styleList.Count; i++)
            {
                if (styleList[i].bindingName == bindingName)
                    return styleList[i];
            }

            return null;
        }

        public InputStyleData GetInputStyleDataSpecific(string bindingName, bool gamepad)
        {
            List<InputStyleData> styleList;

            if (gamepad)
            {
                styleList = inputStyleGamepadDataList;
            }
            else
            {
                styleList = inputStyleKeyboardDataList;
            }

            for (int i = 0; i < styleList.Count; i++)
            {
                if (styleList[i].bindingName == bindingName)
                    return styleList[i];
            }

            return null;
        }

        public string GetBindingName(InputAction action, InputBinding binding)
        {
            string bindingName = action.actionMap.name+"/"+action.name;

            if (!binding.isComposite)
            {
                bindingName += "/" + binding.name;
            }

            return bindingName;
        }

        public string GetSpriteStyleTag(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString : "";
        }

        public string GetSpriteStyleTagSingle(string bindingName)
        {
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString_singleInput : "";
        }

        public string GetSpriteStyleTagSingle(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.inputStyleString_singleInput : "";
        }

        public string GetHumanReadableString(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.humanReadableString : "";
        }

        public string GetHumanReadableStringSingle(InputAction action, InputBinding binding)
        {
            string bindingName = GetBindingName(action, binding);
            InputStyleData data = GetInputStyleData(bindingName);
            return data != null ? data.humanReadableString_singleInput : "";
        }


        public static string GetActionStringRenaming(string name)
        {
            
            for(int i=0; i<Instance.actionNameRenamings.Count; i++)
            {
                if (Instance.actionNameRenamings[i].originalString.ToUpper() == name.ToUpper())
                    return Instance.actionNameRenamings[i].outputString.ToUpper();
            }
            return name;
        }


        public List<string> GetAllBindingNames()
        {
            List<string> output = new List<string>();
            for(int i=0; i<inputStyleKeyboardDataList.Count; i++)
            {
                output.Add(inputStyleKeyboardDataList[i].bindingName);
            }

            for (int i = 0; i < inputStyleGamepadDataList.Count; i++)
            {
                if(!output.Contains(inputStyleGamepadDataList[i].bindingName))
                    output.Add(inputStyleGamepadDataList[i].bindingName);
            }
            return output;
        }

      
        public static InputDevice GetCurrentInputDevice()
        {
            return Instance.currentInputDevice;
        }


        public static void UpdateTMProStyleSheetWithUsedPlayerInputs()
        {

            InputIconSetBasicSO iconSetSO = InputIconSetConfiguratorSO.GetCurrentIconSet();
            if (iconSetSO == null)
                return;

            iconSetSO.OverrideStylesInStyleSheet();
        }

        public static void UpdateStyleData()
        {
            Instance.CreateInputStyleData();
            UpdateTMProStyleSheetWithUsedPlayerInputs();
        }

        public static void RegisterInputIconsText(InputIconsText inputIconsText)
        {
            activeTexts.Add(inputIconsText);
            inputIconsText.SetDirty();
        }

        public static void UnregisterInputIconsText(InputIconsText inputIconsText)
        {
            activeTexts.Remove(inputIconsText);
        }

        public static void RefreshInputIconsTexts()
        {
            foreach(InputIconsText iconsText in activeTexts)
            {
                iconsText.SetDirty();
            }
        }

        public static void SaveUserBindings()
        {
            if (!Instance.loadAndSaveInputBindingOverrides)
                return;

            OverrideBindingDataWrapperClass bindingList = new OverrideBindingDataWrapperClass();

            foreach(InputActionAsset asset in Instance.usedActionAssets)
            {
                foreach(InputActionMap map in asset.actionMaps)
                {
                    foreach (InputBinding binding in map.bindings)
                    {
                        if (!string.IsNullOrEmpty(binding.overridePath))
                        {
                            bindingList.bindingList.Add(new OverrideBindingData(binding.id.ToString(), binding.overridePath));
                        }
                    }
                }
            }

            PlayerPrefs.SetString("II-Rebinds_", JsonUtility.ToJson(bindingList));
            PlayerPrefs.Save();
        }

        public static void LoadUserRebinds()
        {
            if (!Instance.loadAndSaveInputBindingOverrides)
            {
                Instance.CreateInputStyleData();
                return;
            }
           
            if (PlayerPrefs.HasKey("II-Rebinds_"))
            {
                OverrideBindingDataWrapperClass bindingList = JsonUtility.FromJson ( PlayerPrefs.GetString ( "II-Rebinds_" ), typeof (OverrideBindingDataWrapperClass) ) as OverrideBindingDataWrapperClass;

                //create a dictionary to easier check for existing overrides
                Dictionary<System.Guid, string> overrides = new Dictionary<System.Guid, string> ();
                foreach (OverrideBindingData item in bindingList.bindingList)
                {
                    overrides.Add(new System.Guid(item.id), item.path);
                }

                //walk through action maps check dictionary for overrides
                foreach (InputActionAsset asset in Instance.usedActionAssets)
                {
                    foreach (InputActionMap map in asset.actionMaps)
                    {
                        var bindings = map.bindings;
                        for (int i = 0; i < bindings.Count; ++i)
                        {
                            if (overrides.TryGetValue(bindings[i].id, out string overridePath))
                            {
                                //if there is an override apply it
                                map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                            }
                        }
                    }
                }
            }

            Instance.CreateInputStyleData();
        }


        /// <summary>
        /// Can be used to apply binding overrides to the used Input Action Assets.
        /// Use this if you don't use the Rebind Prefabs that come with this asset,
        /// or if you apply binding overrides somewhere else and they should be reflected by Input Icons.
        /// </summary>
        public static void ApplyBindingOverridesToInputActionAssets(InputAction action, int bindingIndex, string overridePath)
        {
            bool bindingChanged = false;
            foreach (InputActionAsset inputActionAsset in Instance.usedActionAssets)
            {
                foreach (InputActionMap actionMap in inputActionAsset.actionMaps)
                {
                    InputAction otherAction = actionMap.FindAction(action.id);
                    if (otherAction != null)
                    {
                        InputBinding otherBinding = otherAction.bindings[bindingIndex];
                        if (otherBinding.overridePath != null && overridePath == null)
                        {
                            //Override Path was removed
                            otherAction.RemoveBindingOverride(bindingIndex);
                            bindingChanged = true;
                        }
                        else if (otherBinding.overridePath == null && overridePath != null)
                        {
                            //Path was overriden
                            otherAction.ApplyBindingOverride(bindingIndex, overridePath);
                            bindingChanged = true;
                        }
                        else if (otherBinding.overridePath != overridePath)
                        {
                            //Path was changed
                            otherAction.ApplyBindingOverride(bindingIndex, overridePath);
                            bindingChanged = true;
                        }

                        break;
                    }
                }
            }

            if (bindingChanged)
            {
                HandleInputBindingsChanged();
            }
        }


        /// <summary>
        /// Private wrapper class for json serialization of the overrides
        /// </summary>
        [System.Serializable]
        class OverrideBindingDataWrapperClass
        {
            public List<OverrideBindingData> bindingList = new List<OverrideBindingData> ();
        }

        [Serializable]
        private class OverrideBindingData
        {
            public string id;
            public string path;

            public OverrideBindingData(string bindingId, string bindingPath)
            {
                id = bindingId;
                path = bindingPath;
            }
        }

    }
}
