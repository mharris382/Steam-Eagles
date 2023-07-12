using CoreLib.Structures;

namespace AI.Bots
{
    public interface IWhiteList
    {
        bool IsWhitelisted(Target target);
    }
}