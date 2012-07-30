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
    class BufferedConsumer : IDataConsumer
    {
        byte[] buffer;
        int bufferBytesUsed = 0;
        Action<BodyContents> resultCallback;
        Action<Exception> errorCallback;
        string _contentType;

        public BufferedConsumer(Action<BodyContents> resultCallback, Action<Exception> errorCallback, string contentType, int contentSize)
        {
            this.resultCallback = resultCallback;
            this.errorCallback = errorCallback;
            _contentType = contentType;
            buffer = new byte[contentSize];
        }

        //Sample code just kept references to arraysegments and then merged them in OnEnd
        //However, when receiving photos all bytes are 0x00 in OnEnd
        public bool OnData(ArraySegment<byte> data, Action continuation)
        {
            if (bufferBytesUsed + data.Count > buffer.Length)
                Array.Resize(ref buffer, bufferBytesUsed + data.Count);

            Buffer.BlockCopy(data.Array, data.Offset, buffer, bufferBytesUsed, data.Count);
            bufferBytesUsed += data.Count;
            return false;
        }
        public void OnError(Exception error)
        {
            errorCallback(error);
        }

        public void OnEnd()
        {
            if(bufferBytesUsed!=buffer.Length)
                Array.Resize(ref buffer, bufferBytesUsed);              
            resultCallback(new BodyContents(buffer, _contentType));
        }
    }
}
