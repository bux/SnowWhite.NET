using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kayak.IosHttp
{

    /// <summary>
    /// Wraps an IDataConsumer. Buffers up all data before passing on to wrapped consumer.
    /// </summary>
    class BufferedConsumerWrapper : IDisposable, IDataConsumer
    {
        IDataConsumer _wrappedConsumer = null;
        List<ArraySegment<byte>> _buffer = new List<ArraySegment<byte>>();

        public void Dispose()
        {
            var wr = _wrappedConsumer as IDisposable;
            if (wr != null)
                wr.Dispose();
        }

        internal BufferedConsumerWrapper(IDataConsumer wrappedConsumer)
        {
            _wrappedConsumer = wrappedConsumer;
        }

        public void OnError(Exception e)
        {
            _wrappedConsumer.OnError(e);
        }

        public bool OnData(ArraySegment<byte> data, Action continuation)
        {
            //No idea what to do with continuation. Am I supposed to call it?
            _buffer.Add(data);
            return false;
        }

        static byte[] ConvertToByteArray(IList<ArraySegment<byte>> list)
        {
            var bytes = new byte[list.Sum(asb => asb.Count)];
            int pos = 0;

            foreach (var asb in list)
            {
                Buffer.BlockCopy(asb.Array, asb.Offset, bytes, pos, asb.Count);
                pos += asb.Count;
            }

            return bytes;
        }

        public void OnEnd()
        {
            byte[] allbytes = ConvertToByteArray(_buffer);
            _buffer.Clear();
            _wrappedConsumer.OnData(new ArraySegment<byte>(allbytes), null);
            _wrappedConsumer.OnEnd();
        }
    }

}
