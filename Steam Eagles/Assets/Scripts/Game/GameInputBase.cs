using UnityEngine;

namespace Game
{
    public abstract class GameInputBase : MonoBehaviour
    {
        public abstract void UpdateInput(GameObject playerInputGO);
    }
}