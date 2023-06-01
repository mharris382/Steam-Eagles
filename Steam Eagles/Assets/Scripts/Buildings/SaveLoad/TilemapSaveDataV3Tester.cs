using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class TilemapSaveDataV3Tester : MonoBehaviour
{
    private TilemapsSaveDataV3 saveDataV3;

    [Inject]
    public void Install(TilemapsSaveDataV3 saveDataV3)
    {
        this.saveDataV3 = saveDataV3;
    }

    private bool ShowButtons => saveDataV3 != null;
    [Button, ShowIf(nameof(ShowButtons))]
    public void TestLoad()
    {
        StartCoroutine(UniTask.ToCoroutine(async () =>
        {
            Debug.Log("Loading...");
            var data = await saveDataV3.LoadGame();
            if(data)
                Debug.Log("Loaded!");
            else
            {
                Debug.LogError("Failed to load!");
            }
        }));
    }
    [Button, ShowIf(nameof(ShowButtons))]
    public void TestSave()
    {
        StartCoroutine(UniTask.ToCoroutine(async () =>
        {
            Debug.Log("Saving...");
            var data = await saveDataV3.SaveGame();
            if (data)
            {
                Debug.Log("Saved");
            }
            else
            {
                Debug.LogError("Failed to save!");
            }
        }));
    }
}