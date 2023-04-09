using UnityEngine;
using UnityEngine.InputSystem;
using InputIcons;

public class II_UIRebindInputActionResetAllBindings : MonoBehaviour
{

    public InputActionAsset assetToReset;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ResetBindings()
    {
        assetToReset.RemoveAllBindingOverrides();  //requires Input System 1.1.1 or higher
        InputIconsManagerSO.SaveUserBindings();
        InputIconsManagerSO.HandleInputBindingsChanged();
        //InputIconsManagerSO.UpdateTMProStyleSheetWithUsedPlayerInputs();
    }
}
