namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Custom StringWriter that exposes itself with the encoding specified.
    /// A normal <see cref="System.IO.StringWriter"/> always exposes itself as UTF-16.
    /// </summary>
    internal class StringWriterWithEncoding : StringWriter
    {
        Encoding encoding;

        public StringWriterWithEncoding(StringBuilder builder, Encoding encoding)
            : base(builder)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding
        {
            get { return encoding; }
        }
    }
}