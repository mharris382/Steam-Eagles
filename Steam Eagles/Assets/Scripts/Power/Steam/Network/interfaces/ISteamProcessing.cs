using UnityEngine;

namespace Power.Steam.Network
{
    public interface ISteamNetwork
    {
        bool HasPosition(Vector2Int position);
        float GetSteamFlowRate(Vector2Int p1, Vector2Int p2);
        float GetPressureLevel(Vector2Int position);
        float GetTemperature(Vector2Int position);
    }
    public interface ISteamProcessing : ISteamNetwork
    {
        void UpdateSteamState(float deltaTime);
        
        bool IsBlocked(Vector2Int position);
    }
    
    
}