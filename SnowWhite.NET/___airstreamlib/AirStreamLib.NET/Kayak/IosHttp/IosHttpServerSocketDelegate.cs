using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpMachine;
using System.Diagnostics;
using Kayak.Http;

namespace Kayak.IosHttp
{
    /// Exposes the underlying socket to hosting application.
    /// In AirPlay this is connection is used for sending player status back to Ios client.
    class HttpServerTransactionDelegateWrapper : IHttpServerTransactionDelegate
    {

        IHttpServerTransactionDelegate _realDelegate;
        IConnectionWatcher _connectionWatcher;
        ISocket _socket;
        bool _isTwoWay = false;

        public HttpServerTransactionDelegateWrapper(IHttpServerTransactionDelegate realDelegate, 
                                             IConnectionWatcher connectionWatcher,
                                             ISocket socket)
        {
            _realDelegate = realDelegate;
            _connectionWatcher = connectionWatcher;
            _socket = socket;
        }

        public void OnRequest(HttpRequestHead request, bool shouldKeepAlive)
        {
            _realDelegate.OnRequest(request, shouldKeepAlive);
            if (request.Uri.StartsWith("/reverse"))
            {
                _isTwoWay = true;
                if (_connectionWatcher != null)
                    _connectionWatcher.TwoWaySocketAvailable(_socket);
            }
        }

        public bool OnRequestData(ArraySegment<byte> data, Action continuation)
        {
            return _realDelegate.OnRequestData(data, continuation);
        }

        public void OnRequestEnd()
        {
            _realDelegate.OnRequestEnd();
        }

        public void OnError(Exception e)
        {
            _realDelegate.OnError(e);
        }

        public void OnEnd()
        {
            if (_isTwoWay && _connectionWatcher != null)
                _connectionWatcher.TwoWaySocketDisconnected(_socket);
            _realDelegate.OnEnd();
        }

        public IDisposable Subscribe(IObserver<IDataProducer> observer)
        {
            return _realDelegate.Subscribe(observer);
        }
    }


    /// <summary>
    /// transforms socket events into http server transaction events.
    ///
    /// </summary>
    class IosHttpServerSocketDelegate : ISocketDelegate
    {
        HttpParser parser;
        ParserToTransactionTransform transactionTransform;
        IHttpServerTransactionDelegate transactionDelegate;
        IDisposable transactionDelegateSubscription;


        public IosHttpServerSocketDelegate(IHttpServerTransactionDelegate transactionDelegate)
        {
            this.transactionDelegate = transactionDelegate;
            transactionTransform = new ParserToTransactionTransform(transactionDelegate);
            parser = new HttpParser(new ParserDelegate(transactionTransform));
        }

        public void Start(ISocket socket)
        {
            transactionDelegateSubscription = transactionDelegate.Subscribe(new OutputSegmentQueue(socket));
        }

        public virtual bool OnData(ISocket socket, ArraySegment<byte> data, Action continuation)
        {
            try
            {
                //Since HTTP connection is used by server to send HTTP requests (Player status updates)
                //back to Ios client, the HTTP connections will also receive HTTP responses from the client.
                //This is a quick and dirty hack to ignore these response messages.
                bool skipParse = false;
                if (data.Count == 38)
                {
                    if (Encoding.UTF8.GetString(data.Array, 0, 38) == "HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n")
                        skipParse = true;
                }

                var parsed = parser.Execute(data);

                if (parsed != data.Count && !skipParse)
                {
                    Trace.Write("Error while parsing request.");
                    throw new Exception("Error while parsing request.");
                }
                // raises request events on transaction delegate
                return transactionTransform.Commit(continuation);
            }
            catch (Exception e)
            {
                OnError(socket, e);
                OnClose(socket);
                throw;
            }
        }

        public void OnEnd(ISocket socket)
        {
            Debug.WriteLine("Socket OnEnd." + (socket as DefaultKayakSocket).id);

            // parse EOF
            OnData(socket, default(ArraySegment<byte>), null);

            transactionDelegate.OnEnd();
        }

        public void OnError(ISocket socket, Exception e)
        {
            Debug.WriteLine("Socket OnError.");
            e.DebugStackTrace();
            transactionDelegate.OnError(e);
        }

        public virtual void OnClose(ISocket socket)
        {
            Debug.WriteLine("Socket OnClose.");

            socket.Dispose();

            // release (indirect) reference to socket
            transactionDelegateSubscription.Dispose();
            transactionDelegateSubscription = null;

            // XXX return self to freelist
        }

        public void OnConnected(ISocket socket)
        {
            throw new NotImplementedException();
        }

    }
}
