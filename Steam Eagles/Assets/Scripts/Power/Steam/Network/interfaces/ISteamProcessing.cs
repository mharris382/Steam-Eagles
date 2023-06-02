using UnityEngine;

namespace Power.Steam.Network
{
    public interface ISteamProcessing
    {
        void UpdateSteamState(float deltaTime);
        float GetSteamFlowRate(Vector2Int position);
        float GetPressureLevel(Vector2Int position);
        bool IsBlocked(Vector2Int position);
    }
}