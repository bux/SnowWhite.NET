using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kayak.IosHttp
{
    public interface IConnectionWatcher
    {
        void TwoWaySocketAvailable(ISocket socket);
        void TwoWaySocketDisconnected(ISocket socket);

    }
}
