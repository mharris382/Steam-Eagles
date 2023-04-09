using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using InputIcons;
using static InputIcons.InputIconsUtility;
using System.Collections.Generic;
using System.Collections;

public class II_UIRebindInputActionBehaviour : MonoBehaviour
{
    /// <summary>
    /// Reference to the action that is to be rebound.
    /// </summary>
    public InputActionReference actionReference
    {
        get => m_Action;
        set
        {
            m_Action = value;
            UpdateBindingDisplay();
        }
    }

    [Tooltip("Reference to action that is to be rebound from the UI.")]
    [SerializeField]
    private InputActionReference m_Action;

    [SerializeField]
    private BindingType m_BindingType = BindingType.Up;

    [SerializeField]
    private InputIconsUtility.DeviceType m_RebindType = InputIconsUtility.DeviceType.Auto;

    [SerializeField]
    public InputBindingComposite part;


    private string previousBinding;

    public InputIconsUtility.DeviceType deviceType
    {
        get => m_RebindType;
        set
        {
            m_RebindType = value;
            UpdateBindingDisplay();
        }
    }
    
    public BindingType bindingType
    {
        get => m_BindingType;
        set
        {
            m_BindingType = value;
            UpdateBindingDisplay();
        }
    }

    private static InputActionRebindingExtensions.RebindingOperation rebindOperation;

    public delegate void OnRebindOperationCompleted(II_UIRebindInputActionBehaviour rebindBehaviour);
    public static OnRebindOperationCompleted onRebindOperationCompleted;

    public bool canBeRebound = true;

    [Header("UI Display - Action Text")]
    public TextMeshProUGUI actionNameDisplayText;

    [Header("UI Display - Binding Text")]
    public TextMeshProUGUI bindingNameDisplayText;

    [Header("UI Display - Buttons")]
    public GameObject rebindButtonObject;
    public GameObject resetButtonObject;

    [Header("UI Display - Listening Text")]
    public GameObject listeningForInputObject;

    [Header("UI Display - Key Already Used")]
    public GameObject keyAlreadyUsedObject;

    private void Awake()
    {
        onRebindOperationCompleted += HandleAnyRebindOperationCompleted;
        UpdateBehaviour();
    }

    private void OnEnable()
    {
        UpdateBehaviour();
        InputIconsManagerSO.onControlsChanged += HandleControlsChanged;
        InputIconsManagerSO.onBindingsChanged += UpdateBehaviour;
    }

    private void OnDisable()
    {
        InputIconsManagerSO.onControlsChanged -= HandleControlsChanged;
        InputIconsManagerSO.onBindingsChanged -= UpdateBehaviour;
    }

    private void OnDestroy()
    {
        onRebindOperationCompleted -= HandleAnyRebindOperationCompleted;
    }

    public void HandleControlsChanged(InputDevice inputDevice)
    {
        UpdateBehaviour();
    }


    public void ButtonPressedStartRebind()
    {
        if (!canBeRebound)
            return;

        StartRebindProcess();
    }


    void StartRebindProcess()
    {
        if (rebindOperation != null)
            rebindOperation.Cancel();

        InputDevice device = InputIconsManagerSO.GetCurrentInputDevice();

        ToggleGameObjectState(rebindButtonObject, false);
        ToggleGameObjectState(resetButtonObject, false);
        ToggleGameObjectState(listeningForInputObject, true);

        m_Action.action.Disable();

        //do not allow keyboard/mouse buttons, when rebind operation was started with gamepad and vice versa
        //and only allow the correct device to bind when the device for this button is keyboard/mouse or gamepad
        string deviceString = GetActiveDeviceString();

        if (deviceType == InputIconsUtility.DeviceType.KeyboardAndMouse)
        {
            deviceString = InputIconsManagerSO.Instance.controlSchemeName_Keyboard;
            //Debug.Log("rebind for keyboard");
        }

        if (deviceType == InputIconsUtility.DeviceType.Gamepad)
        {
            deviceString = InputIconsManagerSO.Instance.controlSchemeName_Gamepad;
            //Debug.Log("rebind for gamepad");
        }

        int index = GetIndexOfBindingType(m_Action.action, bindingType, deviceString);
        rebindOperation = m_Action.action.PerformInteractiveRebinding(index);
        previousBinding = m_Action.action.bindings[index].path;

        rebindOperation.WithControlsExcluding("<Mouse>/position")
        .WithControlsExcluding("<Mouse>/delta")
        //.WithCancelingThrough("<Keyboard>/escape") //we check for cancel in the OnComplete method for better support on gamepads
        .OnMatchWaitForAnother(0.1f)
        .OnCancel(operation => RebindCanceled())
        .OnComplete(operation => RebindCompleted(index))
           // bug fix for new input system version 1.3.0 in which the operation also cancels through e, which should not happen
           .WithCancelingThrough("an enormous string of absolute gibberish which overrides the default which is escape and causes the annoying bug")
         ;


        if (deviceString == InputIconsManagerSO.Instance.controlSchemeName_Gamepad)
        {
            rebindOperation.WithControlsExcluding("<Keyboard>");
            rebindOperation.WithControlsExcluding("<Mouse>");
        }
        else
        {
            rebindOperation.WithControlsExcluding("<Gamepad>");
        }
        rebindOperation.WithBindingGroup(deviceString);


        rebindOperation.Start();
    }


    void RebindCanceled()
    {
        //Debug.Log("rebind canceled");
        rebindOperation.Dispose();
        rebindOperation = null;
        m_Action.action.Enable();

        ToggleGameObjectState(rebindButtonObject, true);
        ToggleGameObjectState(resetButtonObject, true);
        ToggleGameObjectState(listeningForInputObject, false);
    }

    void RebindCompleted(int bindingIndex)
    {
        bool canceled = rebindOperation.canceled;
        if (!canceled)
        {
            //check for operation cancel
            foreach (var candidate in rebindOperation.candidates)
            {
                //InputIconsLogger.Log(candidate.path);
                if (string.Compare(candidate.path, "/Keyboard/escape", ignoreCase: true) == 0
                    || candidate.path.Contains("/start"))
                {
                    canceled = true;
                    break;
                }
            }
        }

        if(!canceled && InputIconsManagerSO.Instance.rebindBehaviour == InputIconsManagerSO.RebindBehaviour.CancelOverrideIfBindingAlreadyExists)
        {
            if(CheckDuplicateBindings(m_Action, bindingIndex))
            {
                canceled = true;
                keyAlreadyUsedObject.SetActive(true);
                StartCoroutine(DisableAlreadyUsedObject());
            }
        }

        if (canceled)
        {
            m_Action.action.ApplyBindingOverride(bindingIndex, previousBinding);
            RebindCanceled();
            return;
        }


        string device;
        string key;

        for (int i=0; i< m_Action.action.bindings.Count; i++)
        {
            m_Action.action.GetBindingDisplayString(i, out device, out key);
        }

        InputIconsManagerSO.SaveUserBindings();

        rebindOperation.Dispose();
        rebindOperation = null;
        m_Action.action.Enable();

        onRebindOperationCompleted?.Invoke(this);
        InputIconsManagerSO.HandleInputBindingsChanged();

        ToggleGameObjectState(rebindButtonObject, true);
        ToggleGameObjectState(resetButtonObject, true);
        ToggleGameObjectState(listeningForInputObject, false);
    }

    IEnumerator DisableAlreadyUsedObject()
    {
        yield return new WaitForSecondsRealtime(0.7f);
        //InputIconsLogger.Log("set inactive");
        keyAlreadyUsedObject.SetActive(false);
      
    }

    private bool CheckDuplicateBindings(InputAction action, int bindingIndex)
    {
        InputBinding bindingToCheck = action.bindings[bindingIndex];
        if (bindingToCheck == null)
            return false;

        foreach(InputBinding binding in action.actionMap.bindings)
        {
            if (binding.action == bindingToCheck.action) continue;
            if (bindingToCheck.effectivePath == "") continue;

            if (binding.effectivePath == bindingToCheck.effectivePath) //duplicate binding found
            {
                InputIconsLogger.Log("Duplicate binding found: " + action.actionMap.name+" "+ binding.action.ToString() + " "+binding.effectivePath);
                return true;
            }
        }

        return false;

    }

    public void HandleAnyRebindOperationCompleted(II_UIRebindInputActionBehaviour rebindBehaviour)
    {

        if (!canBeRebound)
            return;

        if (rebindBehaviour == this)
            return;

        if (rebindBehaviour.actionReference.action.actionMap != m_Action.action.actionMap)
            return;

        if(rebindBehaviour.actionReference.action == m_Action.action)
        {
            if (ActionIsComposite(rebindBehaviour.actionReference.action))
            {
                if (rebindBehaviour.m_BindingType == m_BindingType)
                    return;
            }
            else
                return; 
        }

        List<InputBinding> newBinding = GetBindings(rebindBehaviour.actionReference, rebindBehaviour.bindingType);
        List<InputBinding> myBindings = GetBindings(m_Action, m_BindingType);

        for(int i=0; i<newBinding.Count; i++)
        {
            for(int j=0; j<myBindings.Count; j++)
            {
                //remove my binding if user bound another action to the same key as my current binding
                if (newBinding[i].effectivePath == myBindings[j].effectivePath)
                {
                    if (newBinding[i].id != myBindings[j].id)
                    {
                        //Debug.Log("bindings are equal: "+rebindBehaviour.bindingType+newBinding[i]+" old: "+m_BindingType+myBindings[j]);
                        int bindingIndex = GetIndexOfInputBinding(m_Action.action, myBindings[j]);
                        m_Action.action.ApplyBindingOverride(bindingIndex, "");
                  
                        InputIconsManagerSO.SaveUserBindings();
                    }
                }
            }
        }
       
    }

    public void ButtonPressedResetBinding()
    {
        ResetBinding();
    }

    public void ResetBinding()
    {
        InputActionRebindingExtensions.RemoveAllBindingOverrides(m_Action.action);
        onRebindOperationCompleted?.Invoke(this);

        InputIconsManagerSO.HandleInputBindingsChanged();
        InputIconsManagerSO.SaveUserBindings();
    }


    public void UpdateBindingDisplay()
    {
        if ((m_Action == null || m_Action.action == null) && Application.isPlaying)
        {
            InputIconsLogger.LogError("No Action assigned, aborting", this);
            return;
        }

        string bindingName = m_Action.action.actionMap.name+"/"+m_Action.action.name;
        
        if(InputIconsUtility.ActionIsComposite(m_Action.action))
        {
            bindingName += "/" + bindingType.ToString().ToLower();
        }

        InputStyleData style;
        if (deviceType == InputIconsUtility.DeviceType.Auto)
        {
            style = InputIconsManagerSO.Instance.GetInputStyleData(bindingName);
        }
        else
        {
            bool gamepad = false;
            if (deviceType == InputIconsUtility.DeviceType.Gamepad)
            {
                gamepad = true;
            }
            style = InputIconsManagerSO.Instance.GetInputStyleDataSpecific(bindingName, gamepad);
        }

        string tmp_Text = "";
        if (style != null) { tmp_Text = style.inputStyleString_singleInput; }
        bindingNameDisplayText.SetText(tmp_Text);

        //bindingNameDisplayText.SetText(InputIconsManagerSO.Instance.GetSpriteStyleTagSingle(bindingName));
    }

    public void UpdateBehaviour()
    {
        UpdateBindingDisplay();

        rebindButtonObject.GetComponent<Button>().interactable = canBeRebound;
    }


    void ToggleGameObjectState(GameObject targetGameObject, bool newState)
    {
        targetGameObject.SetActive(newState);
    }
}
