using System.Collections.Generic;
using UniRx;

namespace Weather.Storms
{
    public class StormRegistry
    {
        private ReactiveCollection<Storm> _activeStorms = new ReactiveCollection<Storm>();
        private Dictionary<string, int> _taggedStorms = new Dictionary<string, int>();
        private Dictionary<Storm, string> _stormTags = new Dictionary<Storm, string>();
        public IEnumerable<Storm> GetActiveStorms() => _activeStorms;

        public int Count => _activeStorms.Count;
       
        public Storm GetStorm(int index) => _activeStorms[index];
        public Storm GetStorm(string tag) =>  _activeStorms[_taggedStorms[tag]];

        internal void AddStorm(Storm storm)
        {
            if (_activeStorms.Contains(storm))
                return;
            _activeStorms.Add(storm);
        }
        internal void AddStorm(Storm storm, string tag)
        {
            if (_activeStorms.Contains(storm) || _stormTags.ContainsKey(storm) || _taggedStorms.ContainsKey(tag))
                return;
            _taggedStorms.Add(tag, _activeStorms.Count);
            _stormTags.Add(storm, tag);
            _activeStorms.Add(storm);
        }
        internal void RemoveStorm(Storm storm)
        {
            if (_stormTags.ContainsKey(storm))
            {
                _taggedStorms.Remove(_stormTags[storm]);
                _stormTags.Remove(storm);
            }
            _activeStorms.Remove(storm);
            storm.Dispose();
        }
    }
}