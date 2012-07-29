using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using Kayak.Http;
using Kayak;

namespace AirStreamLib.Webserver
{
    class ResponseMessageHelper 
    {
        public static HttpResponseHead GetHttpResponseHead(string status = "200 OK")
        {
            var headers = new HttpResponseHead()
            {
                Status = status,
                Headers = new Dictionary<string, string>()
            };
            return headers;
        }

        public static Dictionary<object, object> GetServerInfo()
        {
            //retValue.Add("deviceid", "FF:FF:FF:FF:FF:FF");
            var retValue = new Dictionary<object, object>();
            retValue.Add("deviceid", "58:55:CA:06:BD:9E");
            retValue.Add("features", "0x77");
            retValue.Add("model", "AppleTV2,1");
            retValue.Add("protovers", "1.0");
            retValue.Add("srcvers", "101.10");
            return retValue;
        }

        public static Dictionary<object, object> GetStatusMessage(string status)
        {
            var retValue = new Dictionary<object, object>();
            retValue.Add("state", status);
            return retValue;
        }

        public static Dictionary<object, object> GetPlaybackInfo(double position, double duration, double rate)
        {
            var retValue = new Dictionary<object, object>();
            retValue.Add("duration", duration);

            var loadedTimeRanges = new Dictionary<object, object>();
            loadedTimeRanges.Add("duration", duration);
            loadedTimeRanges.Add("start", 0.0d);
            retValue.Add("loadedTimeRanges", loadedTimeRanges);

            retValue.Add("playbackBufferEmpty", true);
            retValue.Add("playbackBufferFull", false);
            retValue.Add("playbackLikelyToKeepUp", true);
            retValue.Add("position", position);
            retValue.Add("rate", rate);
            retValue.Add("readyToPlay", true);

            var seekableTimeRanges = new Dictionary<object, object>();
            seekableTimeRanges.Add("duration", duration);
            seekableTimeRanges.Add("start", 0.0d);
            retValue.Add("seekableTimeRanges", seekableTimeRanges);

            return retValue;
        }



        public static byte[] GetBytes(HttpRequestHead header, IDictionary pList)
        {
            var plistWriter = new System.Runtime.Serialization.Plists.BinaryPlistWriter();
            string content = System.Runtime.Serialization.Plists.PlistXmlDocument.CreateDocument(pList);

            var sb = new StringBuilder();
            sb.AppendFormat("{0} {1}  /1.1 HTTP/{2}.{3}\r\n",header.Method,header.Path,header.Version.Major,header.Version.Minor);
            if (!string.IsNullOrEmpty(content))
            {
                header.Headers["Content-Length"] = Encoding.UTF8.GetByteCount(content).ToString();
                header.Headers["Content-Type"] = @"text/x-apple-plist+xml";
            }

            foreach (var pair in header.Headers)
                foreach (var line in pair.Value.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                    sb.AppendFormat("{0}: {1}\r\n", pair.Key, line);
            sb.Append("\r\n");
            sb.Append(content);
            sb.Append("\r\n");

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

    }
}
