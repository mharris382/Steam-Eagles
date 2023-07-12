using a;
using AI.Enemies.Systems;
using CoreLib.Structures;
using UnityEngine;

namespace AI.Bots
{
    public class BotTargetFilter : ITargetFilter
    {
        private readonly BotAIContext _context;
        private readonly IWhiteList _whitelist;

        public BotTargetFilter(BotAIContext context, IWhiteList whitelist)
        {
            _context = context;
            _whitelist = whitelist;
        }
        public bool Filter(Target target) => _whitelist.IsWhitelisted(target) == false && _context.targetingConfig.IsDistanceInRange(Vector2.Distance(_context.Position, target.transform.position)) ;
    }
}