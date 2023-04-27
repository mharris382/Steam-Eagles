using JetBrains.Annotations;
using QuikGraph;

namespace Power.Steam
{
    public class SteamFlow : EquatableEdge<SteamNode>
    {
        public SteamFlow([NotNull] SteamNode source, [NotNull] SteamNode target) : base(source, target)
        {
        }
    }
}