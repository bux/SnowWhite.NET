using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Http;

namespace Kayak.IosHttp
{
    class IosHttpResponseDelegateFactory : IHttpResponseDelegateFactory
    {
        public IHttpResponseDelegateInternal Create(HttpRequestHead requestHead, bool shouldKeepAlive, Action end)
        {
            // XXX freelist
            return new IosHttpResponseDelegate(
                   prohibitBody: requestHead.Method.ToUpperInvariant() == "HEAD",
                   shouldKeepAlive: shouldKeepAlive,
                   closeConnection: end);
        }
    }

    class IosHttpResponseDelegate : HttpResponseDelegate
    {
        public IosHttpResponseDelegate(bool prohibitBody, bool shouldKeepAlive, Action closeConnection)
            :base(prohibitBody,shouldKeepAlive,closeConnection)
        {
        }

        /// <summary>
        /// Wraps consumer (MessageConsumer) within a buffered consumer that reads whole message into memory.
        /// Result is that HTTP Headers and Body are sent at same socket.write.
        /// Ios client seems to prefer it this way. Freaks out when payload arrives in different packet.
        /// </summary>
        /// <param name="consumer"></param>
        /// <returns></returns>
        public override IDisposable Connect(IDataConsumer consumer)
        {
            return base.Connect(new BufferedConsumerWrapper(consumer));
        }



    }
}
