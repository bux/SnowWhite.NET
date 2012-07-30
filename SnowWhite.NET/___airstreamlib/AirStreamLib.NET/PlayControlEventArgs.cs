using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirStreamLib
{
    public class PlayControlEventArgs : EventArgs
    {
    }

    public class PlayControlLoadUrlEventArgs : PlayControlEventArgs
    {
        public string Url { get; set; }
        public double StartPosition { get; set; }
    }

    public class PlayControlDisplayImageEventArgs : PlayControlEventArgs
    {
        public System.Drawing.Image Image { get; set; }
    }

    public class PlayControlSeekEventArgs : PlayControlEventArgs
    {
        public double NewPosition { get; set; }
    }

    public class PlayControlSetRateEventArgs : PlayControlEventArgs
    {
        public double NewRate { get; set; }
    }

    public class PlayControlStopEventArgs : PlayControlEventArgs
    {
    }
}
