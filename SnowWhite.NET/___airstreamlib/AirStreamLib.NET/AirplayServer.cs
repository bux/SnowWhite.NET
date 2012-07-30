using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ZeroconfService;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.IO;
using AirStreamLib.Webserver;

namespace AirStreamLib
{

    public class AirStreamServer
    {
        private string _domain = "";
        private const string _serviceName = "_airplay._tcp";
        private string _hostName =  "Air Stream Player";
        private int _port = 9001;

        NetService publishService = null;
        KayakWebServer webServerKayak = null;
        
        public event EventHandler<PlayControlEventArgs> PlayControl;
        public event EventHandler<PlayInfoEventArgs> PlayInfo;
        
        public AirStreamServer(string hostName, int port)
        {
            _port = port;
            _hostName = hostName;
            webServerKayak = new KayakWebServer(_port);
            webServerKayak.RequestReceived += new EventHandler<RequestReceivedEventArgs>(webServerKayak_RequestReceived);
        }

        public void StartServer()
        {
            webServerKayak.StartServer();

            publishService = new NetService(_domain, _serviceName, _hostName, _port);
            publishService.DidPublishService += new NetService.ServicePublished(publishService_DidPublishService);
            publishService.DidNotPublishService += new NetService.ServiceNotPublished(publishService_DidNotPublishService);
            publishService.TXTRecordData = NetService.DataFromTXTRecordDictionary(ResponseMessageHelper.GetServerInfo());
            publishService.Publish();
        }

        public void StopServer()
        {
            if (publishService != null)
            {
                publishService.Stop();
            }
            webServerKayak.StopServer();
        }

        public void SendStatus(string status)
        {
            var message = ResponseMessageHelper.GetStatusMessage(status);
            webServerKayak.SendRequestToDevice(message);
        }

        void webServerKayak_RequestReceived(object sender, RequestReceivedEventArgs e)
        {
            var cultureInfoEn = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            PlayControlEventArgs controlEventArgs = null;

            if (e.RequestHead.Uri.StartsWith("/reverse"))
            {
                e.ResponseHead = ResponseMessageHelper.GetHttpResponseHead("101 Switching Protocols");
                e.ResponseHead.Headers["Upgrade"] = "PTTH/1.0";
                e.ResponseHead.Headers["Connection"] = "Upgrade";
            } 
            else if (e.RequestHead.Uri.StartsWith("/server-info") && e.RequestHead.Method == "GET")
            {
                var dict = ResponseMessageHelper.GetServerInfo();
                string content = System.Runtime.Serialization.Plists.PlistXmlDocument.CreateDocument(dict);
                e.ResponseHead.Headers["ContentType"] = @"text/x-apple-plist+xml";
                e.ResponseBodyProducer = new BufferedProducer(content);
            }
            else if (e.RequestHead.Uri.StartsWith("/playback-info") && e.RequestHead.Method == "GET")
            {
                PlayInfoEventArgs ea = new PlayInfoEventArgs();
                if (PlayInfo != null)
                {
                    PlayInfo(this, ea);
                }
                var dict = ResponseMessageHelper.GetPlaybackInfo(ea.Position, ea.Duration, ea.Rate);
                string content = System.Runtime.Serialization.Plists.PlistXmlDocument.CreateDocument(dict);
                e.ResponseHead.Headers["ContentType"] = @"text/x-apple-plist+xml";
                e.ResponseBodyProducer = new BufferedProducer(content);
            }
            else if (e.RequestHead.Uri.StartsWith("/slideshow-features") && e.RequestHead.Method == "GET")
            //photo: get features of this device (transitions, etc.)
            {
            }
            else if (e.RequestHead.Uri.StartsWith("/scrub") && e.RequestHead.Method == "GET")
            {
                PlayInfoEventArgs ea = new PlayInfoEventArgs();
                if (PlayInfo != null)
                {
                    PlayInfo(this, ea);
                }

                string responsedata = String.Format(cultureInfoEn,"duration: {0:0.000000}\nposition: {1:0.000000}", ea.Duration, ea.Position);
                e.ResponseHead.Headers["ContentType"] = @"text/html; charset=UTF-8";
                e.ResponseBodyProducer = new BufferedProducer(responsedata);
            }
            else if (e.RequestHead.Uri.StartsWith("/scrub")  && e.RequestHead.Method == "POST") //seek.
            {
                Regex regex = new Regex(@"position=([0-9\.]+)");
                Match match = regex.Match(e.RequestHead.QueryString);
                if (match.Success)
                {
                    double scrubPosition = Convert.ToDouble(match.Groups[1].ToString(), cultureInfoEn);
                    controlEventArgs = new PlayControlSeekEventArgs() { NewPosition = scrubPosition };
                }
            }
            else if (e.RequestHead.Uri.StartsWith("/rate")  && e.RequestHead.Method == "POST") // play/pause
            {
                Regex regex = new Regex(@"value=([0-9\.]+)");
                Match match = regex.Match(e.RequestHead.QueryString);
                if (match.Success)
                {
                    double rate = Convert.ToDouble(match.Groups[1].ToString(), cultureInfoEn);
                    controlEventArgs = new PlayControlSetRateEventArgs() { NewRate = rate };
                }
            }
            else if (e.RequestHead.Uri.StartsWith("/play")  && e.RequestHead.Method == "POST") // URL og position is in body
            {
                var contentDict = e.RequestBody.GetContentAsDictionary();
                var contentLocation = contentDict["Content-Location"].ToString();
                double startPosition = Convert.ToDouble(contentDict["Start-Position"], cultureInfoEn);
                controlEventArgs = new PlayControlLoadUrlEventArgs() { StartPosition = startPosition, Url = contentLocation };
            }
            else if (e.RequestHead.Uri.StartsWith("/stop")  && e.RequestHead.Method == "POST") 
            {
                controlEventArgs = new PlayControlStopEventArgs();
            }
            else if (e.RequestHead.Uri.StartsWith("/authorize")  && e.RequestHead.Method == "POST") 
            {
            }

            else if (e.RequestHead.Uri.StartsWith("/photo") && e.RequestHead.Method=="PUT")        //photo: show (content is raw jpeg data)
            {
                using (MemoryStream content = e.RequestBody.GetMemoryStream())
                {
                    controlEventArgs = new PlayControlDisplayImageEventArgs()
                    {
                        Image = System.Drawing.Image.FromStream(content)
                    };
                }
            }
            else if (e.RequestHead.Uri.StartsWith("/setProperty") && e.RequestHead.Method=="PUT")
            {
            }
            else   //Unknown request
            {

            }

            if (PlayControl != null && controlEventArgs != null)
                PlayControl(this, controlEventArgs);
        }

        void publishService_DidNotPublishService(NetService service, DNSServiceException exception)
        {
        }

        void publishService_DidPublishService(NetService service)
        {
        }

    }
}
