﻿namespace Spaces
{
    public class WireTile : EditableTile
    {
        public override bool CanTileBeDisconnected()
        {
            return true;
        }
    }
}