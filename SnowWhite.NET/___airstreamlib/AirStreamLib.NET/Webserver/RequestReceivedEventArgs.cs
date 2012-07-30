using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Http;
using Kayak;

namespace AirStreamLib.Webserver
{
    public class RequestReceivedEventArgs : EventArgs
    {
        public HttpRequestHead RequestHead { get; set; }
        public BodyContents RequestBody { get; set; }
        public HttpResponseHead ResponseHead { get; set; }
        public IDataProducer ResponseBodyProducer { get; set; }
    }
}
