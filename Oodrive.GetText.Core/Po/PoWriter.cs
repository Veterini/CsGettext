using System;
using System.Collections.Generic;
using System.IO;

namespace Oodrive.GetText.Core.Po
{
    public class PoWriter : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly IList<IPoEntryWriter> _nodes = new List<IPoEntryWriter>(); 

        public PoWriter(StreamWriter writer)
        {
            _writer = writer;
        }

        private PoHeaderWriter _header;
        public PoHeaderWriter Header => _header ?? (_header = new PoHeaderWriter(_writer));

        public void AddNormalEntry(string id, string value)
        {
            _nodes.Add(new PoEntryWriter(_writer, id, value));
        }

        public void AddContextualEntry(string id, string context, string value)
        {
            _nodes.Add(new PoContextualEntryWriter(_writer, id, context, value));
        }

        public void Flush()
        {
            _header?.Flush();
            foreach (var node in _nodes)
            {
                node?.Flush();
            }
            _writer.Flush();
        }

        public void Dispose()
        {
            Flush();
            _writer.Close();
        }
    }
}
