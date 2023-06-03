using UnityEngine;
using Zenject;

namespace Buildings.SaveLoad
{
    public class BuildingSaveDataWrapper : MonoBehaviour
    {
        public TilemapsSaveDataV3 saveDataV3 { get; private set; }

        [Inject]
        public void InjectMe(TilemapsSaveDataV3 saveDataV3)
        {
            this.saveDataV3 = saveDataV3;
        }
    }
}