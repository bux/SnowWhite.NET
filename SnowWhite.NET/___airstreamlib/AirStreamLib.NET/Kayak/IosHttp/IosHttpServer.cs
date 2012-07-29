using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak.Http;

namespace Kayak.IosHttp
{
    public static class HttpServerExtensions
    {
        public static IServer CreateIosHttp(this IServerFactory factory, IHttpRequestDelegate channel, IConnectionWatcher connWatcher, IScheduler scheduler)
        {
            var f = new IosHttpServerFactory(factory, connWatcher);
            return f.Create(channel, scheduler);
        }
    }

    class IosHttpServerFactory : IHttpServerFactory
    {
        IServerFactory serverFactory;
        IConnectionWatcher connWatcher;

        public IosHttpServerFactory(IServerFactory serverFactory, IConnectionWatcher connWatcher)
        {
            this.serverFactory = serverFactory;
            this.connWatcher = connWatcher;
        }

        public IServer Create(IHttpRequestDelegate del, IScheduler scheduler)
        {
            return serverFactory.Create(new IosHttpServerDelegate(del, connWatcher), scheduler);
        }
    }

    class IosHttpServerDelegate : IServerDelegate
    {
        IHttpRequestDelegate requestDelegate;
        IHttpResponseDelegateFactory responseFactory;
        IConnectionWatcher connWatcher;

        public IosHttpServerDelegate(IHttpRequestDelegate requestDelegate, IConnectionWatcher connWatcher)
        {
            this.requestDelegate = requestDelegate;
            this.responseFactory = new HttpResponseDelegateFactory();
            this.connWatcher=connWatcher;
        }

        public ISocketDelegate OnConnection(IServer server, ISocket socket)
        {
            var txDel = new HttpServerTransactionDelegate(socket.RemoteEndPoint.Address, responseFactory, requestDelegate);
            var delWrapper = new HttpServerTransactionDelegateWrapper(txDel, connWatcher, socket);
            var socketDelegate = new IosHttpServerSocketDelegate(delWrapper);
            socketDelegate.Start(socket);
            return socketDelegate;
        }

        public void OnClose(IServer server)
        {

        }
    }

        
}
