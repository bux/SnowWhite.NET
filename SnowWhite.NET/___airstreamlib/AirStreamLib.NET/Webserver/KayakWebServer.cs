using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using Kayak.Http;
using Kayak.IosHttp;
using Kayak;


namespace AirStreamLib.Webserver
{
    public class KayakWebServer : IHttpRequestDelegate, ISchedulerDelegate, IConnectionWatcher
    {
        Thread kayakThread;
        IScheduler scheduler;
        bool isRunning = false;
        int _port = 8080;

        ISocket callBackSocket = null;

        public event EventHandler<RequestReceivedEventArgs> RequestReceived;

        public KayakWebServer(int port=8080)
        {
            kayakThread = new Thread(new ThreadStart(StartKayakThread));
            kayakThread.IsBackground = true;
            scheduler = KayakScheduler.Factory.Create(this);
            _port = port;
        }


        public void StartServer()
        {
            this.kayakThread.Start();
        }

        public void StopServer()
        {
            scheduler.Post(scheduler.Stop);
        }

        private void StartKayakThread()
        {
            var server = KayakServer.Factory.CreateIosHttp(this, this,scheduler);
            using (server.Listen(new IPEndPoint(IPAddress.Any, _port)))
            {
                // runs scheduler on calling thread. this method will block until
                // someone calls Stop() on the scheduler.
                isRunning = true;
                scheduler.Start();
                isRunning = false;
            }
        }



        private void SendRequest(byte[] requestBytes)
        {
            if (callBackSocket != null)
            {
                callBackSocket.Write(new ArraySegment<byte>(requestBytes), null);
            }
        }

        public void SendRequestToDevice(System.Collections.IDictionary plistBody)
        {
            HttpRequestHead head = new HttpRequestHead();
            head.Headers = new Dictionary<string, string>();
            head.Method="POST";
            head.Path="/event";
            head.Version=new Version(1,1);
            var requestBytes=ResponseMessageHelper.GetBytes(head, plistBody);
            this.scheduler.Post(() => SendRequest(requestBytes));           
        }

        #region IHttpRequestDelegate implementation
        public void OnRequest(HttpRequestHead request, IDataProducer requestBody, IHttpResponseDelegate response)
        {
            var ea = new RequestReceivedEventArgs();
            ea.RequestHead = request;
            ea.ResponseHead = ResponseMessageHelper.GetHttpResponseHead();

            string contentType = string.Empty;
            if(ea.RequestHead.Headers.ContainsKey("Content-Type"))
                contentType=ea.RequestHead.Headers["Content-Type"];
            int contentSize = 0;
            if(ea.RequestHead.Headers.ContainsKey("Content-Length"))
            {
                int.TryParse(ea.RequestHead.Headers["Content-Length"],out contentSize);
            }
            BufferedConsumer bc=new BufferedConsumer(bodyContents =>
                {
                    try
                    {
                        ea.RequestBody = bodyContents;
                        //Called when request body is read to end
                        if (RequestReceived != null)
                        {
                            RequestReceived(this, ea);
                        }
                        var bp = ea.ResponseBodyProducer as BufferedProducer;
                        if (bp != null)
                        {
                            ea.ResponseHead.Headers["Content-Length"] = bp.GetContentLength().ToString();
                        }
                    }
                    finally
                    {
                        response.OnResponse(ea.ResponseHead, ea.ResponseBodyProducer);
                    }
                }, error =>
                {
                }, contentType,contentSize);
            //Gets complete HTTP Request and runs code defined over
            requestBody.Connect(bc);
        }
        #endregion

        #region IConnectionWatcher implementation
        public void TwoWaySocketAvailable(ISocket socket)
        {
            callBackSocket = socket;
        }

        public void TwoWaySocketDisconnected(ISocket socket)
        {
            if (this.callBackSocket == socket)
                this.callBackSocket = null;
        }
        #endregion

        #region ISchedulerDelegate implementation
        public void OnException(IScheduler scheduler, Exception e)
        {
        }

        public void OnStop(IScheduler scheduler)
        {
        }
        #endregion
    }









}
