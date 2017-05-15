using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SteadyBuild
{
    public class TextWriterDemuxer : System.IO.TextWriter
    {
        private readonly IEnumerable<System.IO.TextWriter> _writers;
        private readonly Encoding _encoding;

        public TextWriterDemuxer(Encoding encoding, params System.IO.TextWriter[] writers)
        {
            _writers = writers;
            _encoding = encoding;

            if (!_writers.All(x => x.Encoding == encoding))
            {
                throw new InvalidOperationException("Encoding mismatch.");
            }
        }

        public override void Write(char value)
        {
            foreach (var writer in _writers)
            {
                writer.Write(value);
            }
        }

        public override Encoding Encoding
        {
            get
            {
                return _encoding;
            }
        }

        public override void Flush()
        {
            foreach (var writer in _writers)
            {
                writer.Flush();
            }
        }
    }
}
