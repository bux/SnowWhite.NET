using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using Kayak.Http;
using Kayak;

namespace AirStreamLib.Webserver
{
    public class BodyContents
    {
        byte[] _buffer;
        public string ContentType { get; set; }

        public BodyContents(byte[] buffer, string contentType)
        {
            _buffer = buffer;
            ContentType = contentType;
        }

        public System.IO.MemoryStream GetMemoryStream()
        {
            var memStream = new System.IO.MemoryStream(_buffer);
            return memStream;
        }

        public string GetString()
        {
            return Encoding.UTF8.GetString(_buffer);
        }

        public System.Collections.IDictionary GetContentAsDictionary()
        {
            System.Collections.IDictionary retValue = null;
            if (string.IsNullOrEmpty(ContentType))
            {
                retValue = new Dictionary<object, object>();
                var strings = GetString().Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in strings)
                {
                    int seperatorLinePos = line.IndexOf(':');
                    if (seperatorLinePos > 0)
                    {
                        string firstPart = line.Substring(0, seperatorLinePos);
                        string lastPart = line.Substring(seperatorLinePos + 1);
                        retValue.Add(firstPart.Trim(), lastPart.Trim());
                    }

                }
            }
            else if (ContentType == "application/x-apple-binary-plist")
            {
                System.Runtime.Serialization.Plists.BinaryPlistReader r = new System.Runtime.Serialization.Plists.BinaryPlistReader();
                using (var mStream = GetMemoryStream())
                    retValue = r.ReadObject(mStream);
            }
            return retValue;
        }
    }
}
