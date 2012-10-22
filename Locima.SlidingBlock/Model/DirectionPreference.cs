using System.Collections.Generic;

namespace Locima.SlidingBlock.Model
{
    public class DirectionPreference
    {
        public List<TileDirection> Preferences { get; private set; }

        public DirectionPreference()
        {
            Preferences = new List<TileDirection>();
        }

        public DirectionPreference(IEnumerable<TileDirection> preferences)
        {
            Preferences = new List<TileDirection>(preferences);
        }

    }
}