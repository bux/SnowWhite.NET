using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AirStreamLib
{
    public class PlayInfoEventArgs : EventArgs
    {
        public double Position { get; set; }
        public double Duration { get; set; }
        public double Rate { get; set; }
    }
}
