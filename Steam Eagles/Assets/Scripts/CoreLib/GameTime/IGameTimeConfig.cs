using CoreLib.Interfaces;

namespace CoreLib.GameTime
{
    public interface IGameTimeConfig : IConfig
    {
        float RealSecondsInGameMinute { get; }
    }
    
}